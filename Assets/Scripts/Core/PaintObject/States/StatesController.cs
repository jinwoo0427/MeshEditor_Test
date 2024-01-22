using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;
using GetampedPaint.Core.Layers;

namespace GetampedPaint.States
{
    [Serializable]
    public class StatesController : IStatesController
    {
        public event Action<RenderTexture> OnClearTextureAction;
        public event Action OnRenderTextureAction;
        public event Action OnChangeState;
        public event Action OnResetState;
        public event Action OnUndo;
        public event Action OnRedo;
        public event Action<bool> OnUndoStatusChanged;
        public event Action<bool> OnRedoStatusChanged;
        
        private bool isUndoProcessing;
        public bool IsUndoProcessing => isUndoProcessing;
        
        private bool isRedoProcessing;
        public bool IsRedoProcessing => isRedoProcessing;

        private List<List<BaseChangeRecord>> statesGroups = new List<List<BaseChangeRecord>>();
        private List<RenderTextureChangeRecord> dirtyRenderTextureRecords = new List<RenderTextureChangeRecord>();
#if UNITY_EDITOR && XDP_DEBUG
        public List<RenderTextureChangeRecord> texturesStates = new List<RenderTextureChangeRecord>();
#endif
        
        private ILayersController layersController;
        private List<BaseChangeRecord> currentGroup;
        [SerializeField] private int currentGroupIndex = -1;
        private int minUndoStatesCount;
        private bool isUndoRedo;
        private bool isGroupingEnabled;
        private bool isEnabled;

        public int ChangesCount => statesGroups.Count;

        public IStatesController GetStatesController()
        {
            return this;
        }

        public void DoDispose()
        {
            foreach (var stateGroup in statesGroups)
            {
                foreach (var state in stateGroup)
                {
                    if (state is RenderTextureChangeRecord renderTextureChangeRecord)
                    {
                        renderTextureChangeRecord.DoDispose();
                    }
                }
            }
            statesGroups.Clear();

            foreach (var dirtyRenderTextureChangeRecord in dirtyRenderTextureRecords)
            {
                dirtyRenderTextureChangeRecord?.DoDispose();
            }
            dirtyRenderTextureRecords.Clear();
            
            currentGroupIndex = -1;
            minUndoStatesCount = 0;
            //Debug.Log("설마 실행이 되지 않겠지");
#if UNITY_EDITOR && XDP_DEBUG
            texturesStates.Clear();
#endif
        }

        public void Init(ILayersController layersControllerInstance)
        {
            layersController = layersControllerInstance;
        }

        public void Enable()
        {
            isEnabled = true;
        }

        public void Disable()
        {
            isEnabled = false;
        }
        
        public void AddState(Action action)
        {
            if (isUndoRedo || !isEnabled)
                return;
            
            var willAddNewGroup = isGroupingEnabled && !statesGroups.Contains(currentGroup) || !isGroupingEnabled;
            if (willAddNewGroup)
            {
                UpdateChanges();
            }
            
            var record = new ActionRecord(action);
            if (isGroupingEnabled)
            {
                if (currentGroup.Count == 0)
                {
                    statesGroups.Add(currentGroup);
                }
                currentGroup.Add(record);
            }
            else
            {
                statesGroups.Add(new List<BaseChangeRecord> { record });
            }
            OnAddState();
        }

        public void AddState(object entity, string property, RenderTexture oldValue, RenderTexture newValue, Texture source)
        {
            if (isUndoRedo || !isEnabled)
                return;
            
            var willAddNewGroup = isGroupingEnabled && !statesGroups.Contains(currentGroup) || !isGroupingEnabled;
            if (willAddNewGroup)
            {
                UpdateChanges();
            }

            var previousRenderTexture = GetPreviousRenderTexture(entity, property);
            if (source == null && previousRenderTexture == null)
            {
                previousRenderTexture = GetPreviousRenderTexture(entity, property);
            }
            
            var record = new RenderTextureChangeRecord(entity, property, oldValue, newValue, previousRenderTexture)
            {
                OnAction = OnRenderTextureAction,
                OnClearTexture = OnClearTextureAction
            };
            
            if (isGroupingEnabled)
            {
                if (currentGroup.Count == 0)
                {
                    statesGroups.Add(currentGroup);
                }
                currentGroup.Add(record);
            }
            else
            {         
                statesGroups.Add(new List<BaseChangeRecord> { record });
            }
            
#if UNITY_EDITOR && XDP_DEBUG
            texturesStates.Add(record);
#endif
            OnAddState();
        }

        public void AddState(object entity, string property, object oldValue, object newValue)
        {
            if (isUndoRedo || !isEnabled)
                return;
            
            var willAddNewGroup = isGroupingEnabled && !statesGroups.Contains(currentGroup) || !isGroupingEnabled;
            if (willAddNewGroup)
            {
                UpdateChanges();
            }
            
            var record = new PropertyChangeRecord(entity, property, oldValue, newValue);
            if (isGroupingEnabled)
            {
                if (currentGroup.Count == 0)
                {
                    statesGroups.Add(currentGroup);
                }
                currentGroup.Add(record);
            }
            else
            {
                statesGroups.Add(new List<BaseChangeRecord> { record });
            }
            OnAddState();
        }

        public void AddState(IList collection, NotifyCollectionChangedEventArgs rawEventArg)
        {
            if (isUndoRedo || !isEnabled)
                return;
            
            var willAddNewGroup = isGroupingEnabled && !statesGroups.Contains(currentGroup) || !isGroupingEnabled;
            if (willAddNewGroup)
            {
                UpdateChanges();
            }
            
            var record = new CollectionChangeRecord(collection, rawEventArg);
            if (isGroupingEnabled)
            {
                if (currentGroup.Count == 0)
                {
                    statesGroups.Add(currentGroup);
                }
                currentGroup.Add(record);
            }
            else
            {
                statesGroups.Add(new List<BaseChangeRecord> { record });
            }
            OnAddState();
        }

        public void Undo()
        {
            if (!isEnabled)
                return;


            isUndoProcessing = true;
            var index = currentGroupIndex - 1;
            if (index >= 0)
            {
                OnResetState?.Invoke();
                OnChangeState?.Invoke();
                isUndoRedo = true;
                for (var i = statesGroups[index].Count - 1; i >= 0; i--)
                {
                    var state = statesGroups[index][i];
                    state.Undo();
                }
                isUndoRedo = false;
            }
            currentGroupIndex = Mathf.Clamp(currentGroupIndex - 1, -1, statesGroups.Count);
            UpdateStatus();
            isUndoProcessing = false;
            OnUndo?.Invoke();
        }

        public void Redo()
        {
            if (currentGroupIndex == statesGroups.Count || !isEnabled)
                return;

            isRedoProcessing = true;
            var index = currentGroupIndex;
            OnChangeState?.Invoke();
            isUndoRedo = true;
            for (var i = 0; i < statesGroups[index].Count; i++)
            {
                var state = statesGroups[index][i];
                state.Redo();
            }
            isUndoRedo = false;
            currentGroupIndex = Mathf.Clamp(index + 1, 0, statesGroups.Count);
            UpdateStatus();
            isRedoProcessing = false;
            OnRedo?.Invoke();
        }

        public int GetUndoActionsCount()
        {
            return currentGroupIndex == -1 ? statesGroups.Count : currentGroupIndex;
        }

        public int GetRedoActionsCount()
        {
            if (currentGroupIndex == -1)
                return -1;

            return statesGroups.Count - currentGroupIndex;
        }
        
        /// <summary>
        /// Returns if can Undo
        /// </summary>
        /// <returns></returns>
        public bool CanUndo()
        {
            return statesGroups.Count > minUndoStatesCount && currentGroupIndex > minUndoStatesCount;
        }

        /// <summary>
        /// Returns if can Redo
        /// </summary>
        /// <returns></returns>
        public bool CanRedo()
        {
            return statesGroups.Count > 0 && currentGroupIndex < statesGroups.Count;
        }

        /// <summary>
        /// 상태 그룹화 활성화 -DisableGrouping()을 호출하기 전의 모든 상태는 동일한 그룹에 속합니다.
        /// </summary>
        public void EnableGrouping()
        {
            if (!isEnabled)
                return;
            
            isGroupingEnabled = true;
            currentGroup = new List<BaseChangeRecord>();
        }

        /// <summary>
        /// Disable states grouping
        /// </summary>
        public void DisableGrouping()
        {
            if (!isEnabled)
                return;

            isGroupingEnabled = false;
            currentGroup = null;
        }

        public void SetMinUndoStatesCount(int count)
        {
            minUndoStatesCount = Mathf.Max(1, count);
        }
        
        private RenderTexture GetPreviousRenderTexture(object entity, string property)
        {
            RenderTexture previousTexture = null;
            var index = Mathf.Clamp(currentGroupIndex, 0, currentGroupIndex);
            if (index > 0 && statesGroups.Count > 0)
            {
                for (var i = index - 1; i >= 0; i--)
                {
                    var group = statesGroups[i];
                    for (var j = group.Count - 1; j >= 0; j--)
                    {
                        var state = statesGroups[i][j];
                        if (state is RenderTextureChangeRecord p && p.Entity == entity && p.Property == property)
                        {
                            previousTexture = p.NewTexture;
                            break;
                        }
                    }
                    if (previousTexture != null)
                    {
                        break;
                    }
                }
            }
            if (previousTexture == null && dirtyRenderTextureRecords.Count > 0)
            {
                foreach (var dirtyRecord in dirtyRenderTextureRecords)
                {
                    if (dirtyRecord.Entity == entity && dirtyRecord.Property == property)
                    {
                        previousTexture = dirtyRecord.NewTexture;
                        break;
                    }
                }
            }
            return previousTexture;
        }

        /// <summary>
        /// 상태 그룹 및 더티 렌더 텍스처 레코드를 관리하여 최대 허용 작업 수를 유지하고, 사용되지 않는 리소스를 해제(dispose)합니다.
        /// </summary>
        private void UpdateChanges()
        {
            if (currentGroupIndex != -1)
            {
                //현재 그룹 인덱스(currentGroupIndex)부터 시작하여 현재 상태 그룹의 모든 상태를 반복합니다.
                //상태가 RenderTextureChangeRecord이고 해당 상태가 더티 렌더 텍스처 레코드 목록(dirtyRenderTextureRecords)에 없으면 해당 상태를 해제(dispose)합니다.
                for (var i = statesGroups.Count - 1; i >= currentGroupIndex; i--)
                {
                    var group = statesGroups[i];
                    foreach (var state in group)
                    {
                        if (state is RenderTextureChangeRecord renderTextureChangeRecord && !dirtyRenderTextureRecords.Contains(renderTextureChangeRecord))
                        {
                            renderTextureChangeRecord.DoDispose();
                        }
                    }
                }

                //현재 그룹 인덱스가 설정된 최대 허용 작업 수(UndoRedoMaxActionsCount)를 초과하는 경우 실행됩니다.
                if (currentGroupIndex >= StatesSettings.Instance.UndoRedoMaxActionsCount)
                {
                    if (statesGroups.Count > 0)
                    {
                        //첫 번째 그룹의 각 상태에 대한 검사를 수행합니다.
                        var firstGroup = statesGroups[0];
                        foreach (var groupItem in firstGroup)
                        {
                            //그룹 항목이 RenderTextureChangeRecord인 경우 해당 레코드를 처리합니다.
                            if (groupItem is RenderTextureChangeRecord renderTextureChangeRecord)
                            {
                                //RenderTextureChangeRecord의 이전 텍스처를 해제하고, 더티 렌더 텍스처 레코드 목록을 업데이트합니다.
                                renderTextureChangeRecord.ReleaseOldTexture();
                                var dirtyRenderTextureRecord = dirtyRenderTextureRecords.FirstOrDefault(
                                    x => x.Entity == renderTextureChangeRecord.Entity && x.Property == renderTextureChangeRecord.Property);
                                //해당 레코드가 이미 더티 렌더 텍스처 목록에 있는 경우,
                                //이전 텍스처를 해제하고 목록에서 제거한 후 현재 레코드를 추가합니다.
                                if (dirtyRenderTextureRecord != null)
                                {
                                    dirtyRenderTextureRecord.ReleaseOldTexture();
                                    dirtyRenderTextureRecords.Remove(dirtyRenderTextureRecord);
                                    dirtyRenderTextureRecords.Add(renderTextureChangeRecord);
                                    continue;
                                }
                                dirtyRenderTextureRecords.Add(renderTextureChangeRecord);
                            }

                            //그룹 항목이 CollectionChangeRecord인 경우 해당 레코드를 처리합니다.
                            if (groupItem is CollectionChangeRecord collectionChangeRecord)
                            {
                                //CollectionChangeRecord에서 이전 항목을 찾는 논리를 수행합니다.
                                var foundChanges = false;
                                if (collectionChangeRecord.RawEventArgs.OldItems != null)
                                {
                                    //RawEventArgs.OldItems에 포함된 각 항목에 대해 처리를 수행합니다.
                                    foreach (var layer in collectionChangeRecord.RawEventArgs.OldItems)
                                    {
                                        for (var i = 1; i < statesGroups.Count; i++)
                                        {
                                            var statesGroup = statesGroups[i];
                                            foreach (var record in statesGroup)
                                            {
                                                if (record is CollectionChangeRecord changeRecord)
                                                {
                                                    if (changeRecord.RawEventArgs.OldItems != null)
                                                    {
                                                        foreach (var listItem in changeRecord.RawEventArgs.OldItems)
                                                        {
                                                            if (listItem == layer)
                                                            {
                                                                foundChanges = true;
                                                                break;
                                                            }
                                                        }
                                                    }

                                                    if (!foundChanges && changeRecord.RawEventArgs.NewItems != null)
                                                    {
                                                        foreach (var listItem in changeRecord.RawEventArgs.NewItems)
                                                        {
                                                            if (listItem == layer)
                                                            {
                                                                foundChanges = true;
                                                                break;
                                                            }
                                                        }
                                                    }
                                                }

                                                if (foundChanges)
                                                    break;
                                            }

                                            //이전 항목이 레이어가 아니고 현재 레이어 컨트롤러의 레이어 목록에 없으면 해당 항목을 해제(dispose)하고,
                                            //더티 렌더 텍스처 레코드를 찾아서 업데이트합니다.
                                            if (!foundChanges && !layersController.Layers.Contains(layer))
                                            {
                                                ((ILayer)layer).DoDispose();
                                                var dirtyRecord = dirtyRenderTextureRecords.FirstOrDefault(x => x.Entity == layer);
                                                if (dirtyRecord != null)
                                                {
                                                    dirtyRecord.ReleaseOldTexture();
                                                    dirtyRecord.DoDispose();
                                                    dirtyRenderTextureRecords.Remove(dirtyRecord);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        //더티 렌더 텍스처 레코드 목록을 역순으로 검사하면서 사용되지 않는 레코드를 제거합니다.
                        //실행 취소/다시 실행 스택에 레이어가 없을 때 더티 레코드 제거
                        for (var i = dirtyRenderTextureRecords.Count - 1; i >= 0; i--)
                        {
                            var dirtyRenderTextureRecord = dirtyRenderTextureRecords[i];
                            var found = false;
                            if (dirtyRenderTextureRecord.Entity is ILayer l && layersController.Layers.Contains(l))
                                continue;

                            foreach (var group in statesGroups)
                            {
                                foreach (var state in group)
                                {
                                    if (state is CollectionChangeRecord c)
                                    {
                                        if (c.RawEventArgs.OldItems != null)
                                        {
                                            if (c.RawEventArgs.OldItems.Contains(dirtyRenderTextureRecord.Entity))
                                            {
                                                found = true;
                                                break;
                                            }
                                        }

                                        if (c.RawEventArgs.NewItems != null)
                                        {
                                            if (c.RawEventArgs.NewItems.Contains(dirtyRenderTextureRecord.Entity))
                                            {
                                                found = true;
                                                break;
                                            }
                                        }
                                    }
                                }

                                if (found)
                                {
                                    break;
                                }
                            }

                            if (!found)
                            {
                                dirtyRenderTextureRecords.RemoveAt(i);
                            }
                        }
                    }

                    //최대 허용 작업 수를 초과하는 경우, 상태 그룹 목록에서 첫 번째 그룹을 제거합니다.
                    //혹은 그렇지 않은 경우, 현재 그룹 인덱스 이후의 모든 그룹을 제거합니다.
                    statesGroups = statesGroups.GetRange(1, statesGroups.Count - 1);
                }
                else if (statesGroups.Count > currentGroupIndex)
                {
                    statesGroups = statesGroups.GetRange(0, currentGroupIndex);

                }
                //현재 그룹 인덱스를 상태 그룹 목록의 길이로 업데이트합니다.
                currentGroupIndex = statesGroups.Count;
            }
        }
        private void UpdateStatus()
        {
            OnUndoStatusChanged?.Invoke(CanUndo());
            OnRedoStatusChanged?.Invoke(CanRedo());
        }

        private void OnAddState()
        {
            currentGroupIndex = statesGroups.Count;
            UpdateStatus();
        }
    }
}