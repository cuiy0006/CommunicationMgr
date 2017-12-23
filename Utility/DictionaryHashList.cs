using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Collections;

namespace Utility
{
    public class DictionaryHashList:NameObjectCollectionBase
    {
        private DictionaryEntry _de = new DictionaryEntry();

        public DictionaryHashList()
        { 
        
        }

        public DictionaryHashList(IDictionary d, bool bReadOnly)
        {
            foreach (DictionaryEntry de in d)
            {
                this.BaseAdd((string)de.Key, de.Value);
            }
            this.IsReadOnly = bReadOnly;
        }

        public DictionaryEntry this[int index]
        {
            get
            {
                _de.Key = this.BaseGetKey(index);
                _de.Value = this.BaseGet(index);
                return _de;
            }
        }

        public object this[string key]
        {
            get
            {
                return this.BaseGet(key);
            }
            set
            {
                this.BaseSet(key, value);
            }
        }

        public string[] AllKeys
        {
            get
            {
                return this.BaseGetAllKeys();
            }
        }

        public Array AllValues
        {
            get
            {
                return this.BaseGetAllValues();
            }
        }

        public String[] AllStringValues
        {
            get
            {
                return ((String[])this.BaseGetAllValues(Type.GetType("System.String")));
            }
        }

        public bool HasKeys
        {
            get 
            {
                return this.BaseHasKeys();
            }
        }

        public void Add(string key, object value)
        {
           this.BaseAdd(key, value);
        } 

        public void Remove(string key)
        {
            this.BaseRemove(key);
        }

        public void Remove(int index)
        {
            this.BaseRemoveAt(index);
        }

        public void clear()
        {
            this.BaseClear();
        }
    }
}
