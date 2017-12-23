using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utility
{
    public class DataItems:BaseEntity
    {
        DictionaryHashList mCol;
        private string mvarName;
        private object mvarValue;
        private string tag;

        public string Name
        {
            get { return mvarName; }
            set { mvarName = value; }
        }

        public object Value
        {
            get { return mvarValue; }
            set { mvarValue = value ; }
        }

        public string Tag 
        {
            get { return tag; }
            set { tag = value; }
        }

        public DataItems()
        {
            mCol = new DictionaryHashList(); 
        }

        public void Remove(int Index)
        {
            if (mCol.Count > Index)
            {
                mCol.Remove(Index);
            }
        }

        public void Remove(string Key)
        {
            mCol.Remove(Key);
        }

        public DataItem Add(DataItem NewItem)
        {
            DataItem objNewMember = null;
            try
            {
                objNewMember = NewItem;
                string Name = objNewMember.Name;
                mCol.Add(Name, objNewMember);
            }
            catch (Exception ex)
            {

            }
            return objNewMember;
        }

        public DataItem AddItem(string ItemName)
        {
            DataItem NewItem = new DataItem();
            NewItem.Name = ItemName;
            return Add(NewItem);
        }

        public void CloneAs(DataItems DItems)
        {
            try
            {
                DataItem NewItem;
                DataItem CurrItem;
                this.mCol.clear();
                this.Name = DItems.Name;
                this.Value = DItems.Value;
                for (int i = 0; i < DItems.Count(); i++)
                {
                    NewItem = new DataItem();
                    CurrItem = DItems.Item(i);
                    NewItem.CloneAs(CurrItem);
                    this.Add(NewItem);
                }
            }
            catch (Exception ex)
            { 
                
            }
        }

        public int Count()
        {
            return mCol.Count;
        }

        public bool Exists(string ItemName)
        {
            try
            {
                DataItem DI = (DataItem)mCol[ItemName];
                if (DI == null)
                    return false;
                else
                    return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public DataItem Item(int Index)
        {
            try
            {
                if (mCol.Count > Index)
                {
                    return (DataItem)mCol[Index].Value;
                }
            }
            catch (Exception ex)
            { 
            
            }
            return null;
        }

        public DataItem Item(string key)
        {
            try
            {
                if (!string.IsNullOrEmpty(key))
                {
                    return (DataItem)mCol[key];
                }
            }
            catch (Exception ex)
            { 
            
            }
            return null;
        }

        public override string ToString()
        {
            StringBuilder printstr = new StringBuilder();
            if (mvarName != null)
            {
                string value;
                if (mvarValue == null)
                    value = "*NULL*";
                else if (mvarValue.ToString() == "")
                    value = "*EMPTY*";
                else
                    value = mvarValue.ToString();
                printstr.Append("[" + mvarName + "=" + value + "]\r\n");
            }

            for(int i = 0; i < Count(); i++)
            {
                printstr.Append(Item(i).ToString());
            }
            return printstr.ToString();
        }
    }
}
