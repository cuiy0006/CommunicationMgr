using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utility
{
    public class DataItem:BaseEntity
    {
        private string mvarName;
        private object mvarValue;
        private DataItems mvarChildren;

        public DataItem()
        {
            mvarChildren = new DataItems();
        }

        public string Name
        {
            get { return mvarName; }
            set { mvarName = value; }
        }

        public object Value
        {
            get { return mvarValue; }
            set { mvarValue = value; }
        }

        public DataItems Children
        {
            get { return mvarChildren; }
            set { mvarChildren = value; }
        }

        public void CloneAs(DataItem DItem)
        {
            DataItem CurrChild;
            if (DItem != null)
            {
                this.Name = DItem.Name;
                this.Value = DItem.Value;

                for (int i = 0; i < DItem.Children.Count(); i++)
                {
                    CurrChild = new DataItem();
                    DataItem DChild = DItem.Children.Item(i);
                    CurrChild.CloneAs(DChild);
                    this.Children.Add(CurrChild);
                }
            }
        }

        public override string ToString()
        {
            try
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

                    printstr.Append( "[" + mvarName + "=" + value + "]\r\n");
                }
                printstr.Append(this.Children.ToString());
                return printstr.ToString();
            }
            catch (Exception ex)
            { 
            
            }
            return "";
        }
    }
}
