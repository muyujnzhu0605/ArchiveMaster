using System.Collections;

namespace ArchiveMaster.Basic;

public class UniqueSetList<T> : ISet<T>, IList<T>, IReadOnlyList<T>, IReadOnlySet<T>
{
    private readonly List<T> list = new List<T>();
    private readonly HashSet<T> set = new HashSet<T>();

    public UniqueSetList()
    {
        
    }

    public UniqueSetList(IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            AddImplement(item);
        }
    }
    
    public IEnumerator<T> GetEnumerator()
    {
        return list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private bool AddImplement(T item, int index = -1, bool throwExceptionIfExisted = true)
    {
        if (set.Add(item))
        {
            if (index == -1)
            {
                list.Add(item);
            }
            else
            {
                list.Insert(index, item);
            }

            return true;
        }

        if (throwExceptionIfExisted)
        {
            throw new NotSupportedException("不允许加入重复的项");
        }

        return false;
    }

    void ICollection<T>.Add(T item)
    {
        AddImplement(item);
    }

    public void ExceptWith(IEnumerable<T> other)
    {
        throw new NotSupportedException();
    }

    public void IntersectWith(IEnumerable<T> other)
    {
        throw new NotSupportedException();
    }

    public bool IsProperSubsetOf(IEnumerable<T> other)
    {
        return set.IsProperSubsetOf(other);
    }

    public bool IsProperSupersetOf(IEnumerable<T> other)
    {
        return set.IsProperSupersetOf(other);
    }

    public bool IsSubsetOf(IEnumerable<T> other)
    {
        return set.IsSubsetOf(other);
    }

    public bool IsSupersetOf(IEnumerable<T> other)
    {
        return set.IsSupersetOf(other);
    }

    public bool Overlaps(IEnumerable<T> other)
    {
        return set.Overlaps(other);
    }

    public bool SetEquals(IEnumerable<T> other)
    {
        return set.SetEquals(other);
    }

    public void SymmetricExceptWith(IEnumerable<T> other)
    {
        throw new NotSupportedException();
    }

    public void UnionWith(IEnumerable<T> other)
    {
        foreach (var item in other)
        {
            AddImplement(item, throwExceptionIfExisted: false);
        }

        throw new NotImplementedException();
    }

    bool ISet<T>.Add(T item)
    {
        AddImplement(item);
        return true;
    }

    public void Clear()
    {
        list.Clear();
        set.Clear();
    }

    public bool Contains(T item)
    {
        return set.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        list.CopyTo(array, arrayIndex);
    }

    public bool Remove(T item)
    {
        throw new NotSupportedException("不允许移除单个元素");
    }

    public int Count => set.Count;

    public bool IsReadOnly => false;

    public int IndexOf(T item)
    {
        return list.IndexOf(item);
    }

    public void Insert(int index, T item)
    {
        AddImplement(item, index);
    }

    public void RemoveAt(int index)
    {
        throw new NotSupportedException("不允许移除单个元素");
    }

    public T this[int index]
    {
        get => list[index];
        set => list[index] = value;
    }
}