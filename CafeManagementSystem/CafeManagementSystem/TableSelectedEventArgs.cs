// TableSelectedEventArgs.cs

using System;

namespace CafeManagementSystem
{
    // This is the one and only definition for this class in the entire project.
    public class TableSelectedEventArgs : EventArgs
    {
        public int TableId { get; }
        public string TableName { get; }

        public TableSelectedEventArgs(int tableId, string tableName)
        {
            TableId = tableId;
            TableName = tableName;
        }
    }
}