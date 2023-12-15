using System.Collections;
using System.Collections.Specialized;

namespace XDPaint.States
{
#if UNITY_EDITOR && XDP_DEBUG
    [System.Serializable]
#endif
    public class CollectionChangeRecord : BaseChangeRecord
    {
        private IList list;
        private NotifyCollectionChangedEventArgs rawEventArgs;

        public IList List => list;
        public NotifyCollectionChangedEventArgs RawEventArgs => rawEventArgs;

        public CollectionChangeRecord(IList list, NotifyCollectionChangedEventArgs rawEventArgs)
        {
            this.list = list;
            this.rawEventArgs = rawEventArgs;
        }

        public override void Undo()
        {
            if (rawEventArgs.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var addedItem in rawEventArgs.NewItems)
                {
                    list.Remove(addedItem);
                }
            }
            else if (rawEventArgs.Action == NotifyCollectionChangedAction.Remove)
            {
                var oldIndex = rawEventArgs.OldStartingIndex;
                foreach (var movedItem in rawEventArgs.OldItems)
                {
                    list.Remove(movedItem);
                    list.Insert(oldIndex, movedItem);
                    oldIndex++;
                }
            }
            else if (rawEventArgs.Action == NotifyCollectionChangedAction.Move)
            {
                var oldIndex = rawEventArgs.OldStartingIndex;
                foreach (var movedItem in rawEventArgs.NewItems)
                {
                    list.Remove(movedItem);
                    list.Insert(oldIndex, movedItem);
                    oldIndex++;
                }
            }
        }

        public override void Redo()
        {
            if (rawEventArgs.Action == NotifyCollectionChangedAction.Add)
            {
                var newIndex = rawEventArgs.NewStartingIndex;
                foreach (var movedItem in rawEventArgs.NewItems)
                {
                    list.Remove(movedItem);
                    list.Insert(newIndex, movedItem);
                    newIndex++;
                }
            }
            else if (rawEventArgs.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var removedItem in rawEventArgs.OldItems)
                {
                    list.Remove(removedItem);
                }
            }
            else if (rawEventArgs.Action == NotifyCollectionChangedAction.Move)
            {
                var newIndex = rawEventArgs.NewStartingIndex;
                foreach (var movedItem in rawEventArgs.OldItems)
                {
                    list.Remove(movedItem);
                    list.Insert(newIndex, movedItem);
                    newIndex++;
                }
            }
        }
    }
}