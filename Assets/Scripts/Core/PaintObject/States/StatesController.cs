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
            //Debug.Log("���� ������ ���� �ʰ���");
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
        /// ���� �׷�ȭ Ȱ��ȭ -DisableGrouping()�� ȣ���ϱ� ���� ��� ���´� ������ �׷쿡 ���մϴ�.
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
        /// ���� �׷� �� ��Ƽ ���� �ؽ�ó ���ڵ带 �����Ͽ� �ִ� ��� �۾� ���� �����ϰ�, ������ �ʴ� ���ҽ��� ����(dispose)�մϴ�.
        /// </summary>
        private void UpdateChanges()
        {
            if (currentGroupIndex != -1)
            {
                //���� �׷� �ε���(currentGroupIndex)���� �����Ͽ� ���� ���� �׷��� ��� ���¸� �ݺ��մϴ�.
                //���°� RenderTextureChangeRecord�̰� �ش� ���°� ��Ƽ ���� �ؽ�ó ���ڵ� ���(dirtyRenderTextureRecords)�� ������ �ش� ���¸� ����(dispose)�մϴ�.
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

                //���� �׷� �ε����� ������ �ִ� ��� �۾� ��(UndoRedoMaxActionsCount)�� �ʰ��ϴ� ��� ����˴ϴ�.
                if (currentGroupIndex >= StatesSettings.Instance.UndoRedoMaxActionsCount)
                {
                    if (statesGroups.Count > 0)
                    {
                        //ù ��° �׷��� �� ���¿� ���� �˻縦 �����մϴ�.
                        var firstGroup = statesGroups[0];
                        foreach (var groupItem in firstGroup)
                        {
                            //�׷� �׸��� RenderTextureChangeRecord�� ��� �ش� ���ڵ带 ó���մϴ�.
                            if (groupItem is RenderTextureChangeRecord renderTextureChangeRecord)
                            {
                                //RenderTextureChangeRecord�� ���� �ؽ�ó�� �����ϰ�, ��Ƽ ���� �ؽ�ó ���ڵ� ����� ������Ʈ�մϴ�.
                                renderTextureChangeRecord.ReleaseOldTexture();
                                var dirtyRenderTextureRecord = dirtyRenderTextureRecords.FirstOrDefault(
                                    x => x.Entity == renderTextureChangeRecord.Entity && x.Property == renderTextureChangeRecord.Property);
                                //�ش� ���ڵ尡 �̹� ��Ƽ ���� �ؽ�ó ��Ͽ� �ִ� ���,
                                //���� �ؽ�ó�� �����ϰ� ��Ͽ��� ������ �� ���� ���ڵ带 �߰��մϴ�.
                                if (dirtyRenderTextureRecord != null)
                                {
                                    dirtyRenderTextureRecord.ReleaseOldTexture();
                                    dirtyRenderTextureRecords.Remove(dirtyRenderTextureRecord);
                                    dirtyRenderTextureRecords.Add(renderTextureChangeRecord);
                                    continue;
                                }
                                dirtyRenderTextureRecords.Add(renderTextureChangeRecord);
                            }

                            //�׷� �׸��� CollectionChangeRecord�� ��� �ش� ���ڵ带 ó���մϴ�.
                            if (groupItem is CollectionChangeRecord collectionChangeRecord)
                            {
                                //CollectionChangeRecord���� ���� �׸��� ã�� ���� �����մϴ�.
                                var foundChanges = false;
                                if (collectionChangeRecord.RawEventArgs.OldItems != null)
                                {
                                    //RawEventArgs.OldItems�� ���Ե� �� �׸� ���� ó���� �����մϴ�.
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

                                            //���� �׸��� ���̾ �ƴϰ� ���� ���̾� ��Ʈ�ѷ��� ���̾� ��Ͽ� ������ �ش� �׸��� ����(dispose)�ϰ�,
                                            //��Ƽ ���� �ؽ�ó ���ڵ带 ã�Ƽ� ������Ʈ�մϴ�.
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

                        //��Ƽ ���� �ؽ�ó ���ڵ� ����� �������� �˻��ϸ鼭 ������ �ʴ� ���ڵ带 �����մϴ�.
                        //���� ���/�ٽ� ���� ���ÿ� ���̾ ���� �� ��Ƽ ���ڵ� ����
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

                    //�ִ� ��� �۾� ���� �ʰ��ϴ� ���, ���� �׷� ��Ͽ��� ù ��° �׷��� �����մϴ�.
                    //Ȥ�� �׷��� ���� ���, ���� �׷� �ε��� ������ ��� �׷��� �����մϴ�.
                    statesGroups = statesGroups.GetRange(1, statesGroups.Count - 1);
                }
                else if (statesGroups.Count > currentGroupIndex)
                {
                    statesGroups = statesGroups.GetRange(0, currentGroupIndex);

                }
                //���� �׷� �ε����� ���� �׷� ����� ���̷� ������Ʈ�մϴ�.
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