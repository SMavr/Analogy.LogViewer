﻿using Analogy.DataTypes;
using Analogy.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Analogy.Common.Interfaces;
using Analogy.CommonControls.DataTypes;

namespace Analogy
{
    public class PagingManager
    {
        //public List<string> CurrentColumns { get; set; }
        private static ManualResetEvent columnAdderSync = new ManualResetEvent(false);
        public ReaderWriterLockSlim columnsLockSlim = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private readonly UserControl owner;
        public event EventHandler<AnalogyClearedHistoryEventArgs> OnHistoryCleared;
        public event EventHandler<AnalogyPagingChanged> OnPageChanged;
        public ReaderWriterLockSlim lockSlim = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private IUserSettingsManager Settings => UserSettingsManager.UserSettings;
        private List<DataTable> pages;
        private readonly List<AnalogyLogMessage> allMessages;
        private readonly int pageSize;
        private int currentPageStartRowIndex;
        private int currentPageNumber;

        private DataTable currentTable;
        private static int _totalMissedMessages;

        public int TotalPages
        {
            get
            {
                lockSlim.EnterReadLock();
                var count = pages.Count;
                lockSlim.ExitReadLock();
                return count;

            }
        }

        public static int TotalMissedMessages => _totalMissedMessages;

        public PagingManager(UserControl owner)
        {
            //CurrentColumns = new List<string>();
            this.owner = owner;
            pageSize = Settings.PagingEnabled ? Settings.PagingSize : int.MaxValue;
            pages = new List<DataTable>();

            currentTable = Utils.DataTableConstructor();
            //foreach (DataColumn column in currentTable.Columns)
            //{
            //    CurrentColumns.Add(column.ColumnName);
            //}
            pages.Add(currentTable);
            currentPageNumber = 1;
            allMessages = new List<AnalogyLogMessage>();
        }


        public IEnumerable<List<T>> SplitList<T>(List<T> locations)
        {
            for (int i = 0; i < locations.Count; i += pageSize)
            {
                yield return locations.GetRange(i, Math.Min(pageSize, locations.Count - i));
            }
        }


        public void ClearLogs()
        {
            lockSlim.EnterWriteLock(); 
            var oldMessages = allMessages.ToList();
            try
            {
                pages = new List<DataTable>();
                currentPageStartRowIndex = 0;
                currentPageNumber = 1;
                var first = Utils.DataTableConstructor();
                currentTable = first;
                pages.Add(first);
            }
            finally
            {
                lockSlim.ExitWriteLock();
            }
            OnHistoryCleared?.Invoke(this, new AnalogyClearedHistoryEventArgs(oldMessages));
            AnalogyPageInformation analogyPage = new AnalogyPageInformation(currentTable, 1, currentPageStartRowIndex);
            OnPageChanged?.Invoke(this, new AnalogyPagingChanged(analogyPage));
        }

        public DataRow AppendMessage(AnalogyLogMessage message, string dataSource)
        {
            try
            {
                lockSlim.EnterWriteLock();
                allMessages.Add(message);
            }
            finally
            {
                lockSlim.ExitWriteLock();
            }
            var table = pages.Last();
            if (table.Rows.Count + 1 > pageSize)
            {
                table = Utils.DataTableConstructor();
                pages.Add(table);
                var pageStartRowIndex = (pages.Count - 1) * pageSize;
                OnPageChanged?.Invoke(this, new AnalogyPagingChanged(new AnalogyPageInformation(table, pages.Count, pageStartRowIndex)));
            }

            if (message.AdditionalInformation != null && message.AdditionalInformation.Any() && Settings.CheckAdditionalInformation)
            {
                AddExtraColumnsIfNeeded(table, message);
            }
            try
            {
                lockSlim.EnterWriteLock();
                DataRow dtr = CommonControls.Utils.CreateRow(table, message, dataSource, Settings.TimeOffsetType,Settings.TimeOffset);
                table.Rows.Add(dtr);
                return dtr;

            }
            finally
            {
                lockSlim.ExitWriteLock();
            }
        }

        public List<(DataRow, AnalogyLogMessage)> AppendMessages(List<AnalogyLogMessage> messages, string dataSource)
        {

            var table = pages.Last();
            var countInsideTable = table.Rows.Count;
            List<(DataRow row, AnalogyLogMessage message)> rows = new List<(DataRow row, AnalogyLogMessage message)>(messages.Count);
            foreach (var message in messages)
            {
                if (message.Level == AnalogyLogLevel.None)
                {
                    continue; //ignore those messages
                }
                try
                {
                    lockSlim.EnterWriteLock();
                    allMessages.Add(message);
                }
                finally
                {
                    lockSlim.ExitWriteLock();
                }
                if (countInsideTable + 1 > pageSize)
                {
                    table = Utils.DataTableConstructor();
                    pages.Add(table);
                    countInsideTable = 0;
                    var pageStartRowIndex = (pages.Count - 1) * pageSize;
                    OnPageChanged?.Invoke(this,
                        new AnalogyPagingChanged(new AnalogyPageInformation(table, pages.Count,
                            pageStartRowIndex)));
                }

                if (message.AdditionalInformation != null && message.AdditionalInformation.Any() &&
                    Settings.CheckAdditionalInformation)
                {
                    AddExtraColumnsIfNeeded(table, message);
                }

                countInsideTable++;
                try
                {
                    lockSlim.EnterWriteLock();
                    DataRow dtr =CommonControls.Utils.CreateRow(table, message, dataSource, Settings.TimeOffsetType,Settings.TimeOffset);
                    table.Rows.Add(dtr);
                    rows.Add((dtr, message));
                }
                finally
                {
                    lockSlim.ExitWriteLock();
                }

            }
            return rows;
        }

        private void AddExtraColumnsIfNeeded(DataTable table, AnalogyLogMessage message)
        {
            if (message.AdditionalInformation != null && message.AdditionalInformation.Any() && Settings.CheckAdditionalInformation)
            {
                foreach (KeyValuePair<string, string> info in message.AdditionalInformation)
                {

                    if (!table.Columns.Contains(info.Key))
                    {
                        if (!owner.InvokeRequired)
                        {
                            try
                            {
                                columnsLockSlim.EnterWriteLock();
                                if (!table.Columns.Contains(info.Key))
                                {
                                    table.Columns.Add(info.Key);
                                }
                            }
                            finally
                            {
                                columnsLockSlim.ExitWriteLock();
                            }



                        }
                        else
                        {
                            owner.BeginInvoke(new MethodInvoker(() =>
                            {
                                try
                                {

                                    columnsLockSlim.EnterWriteLock();
                                    if (!table.Columns.Contains(info.Key))
                                    {
                                        columnsLockSlim.EnterWriteLock();
                                        if (!table.Columns.Contains(info.Key))
                                        {
                                            table.Columns.Add(info.Key);
                                            columnAdderSync.Set();
                                        }

                                    }
                                }
                                finally
                                {
                                    columnsLockSlim.ExitWriteLock();
                                }
                            }));
                            columnAdderSync.WaitOne();
                            columnAdderSync.Reset();
                        }
                    }
                }
            }
        }

        public AnalogyPageInformation NextPage()
        {
            lockSlim.EnterReadLock();

            if (pages.Last() != currentTable)
            {
                currentPageNumber++;
                currentTable = pages[currentPageNumber - 1];
                currentPageStartRowIndex = pages.IndexOf(currentTable) * pageSize;
            }
            lockSlim.ExitReadLock();
            return new AnalogyPageInformation(currentTable, currentPageNumber, currentPageStartRowIndex);

        }

        public AnalogyPageInformation PrevPage()
        {
            lockSlim.EnterReadLock();
            if (pages.First() != currentTable)
            {
                currentPageNumber--;
                currentTable = pages[currentPageNumber - 1];
                currentPageStartRowIndex = pages.IndexOf(currentTable) * pageSize;
            }
            lockSlim.ExitReadLock();
            return new AnalogyPageInformation(currentTable, currentPageNumber, currentPageStartRowIndex);

        }

        public DataTable FirstPage()
        {
            lockSlim.EnterReadLock();
            currentTable = pages.First();
            currentPageNumber = 1;
            lockSlim.ExitReadLock();
            return currentTable;

        }
        public DataTable LastPage()
        {
            lockSlim.EnterReadLock();
            currentTable = pages.Last();
            currentPageNumber = pages.Count;
            lockSlim.ExitReadLock();
            return currentTable;

        }

        public List<AnalogyLogMessage> GetAllMessages()
        {
            try
            {
                lockSlim.EnterReadLock();
                var items = allMessages.ToList();
                return items;
            }
            finally
            {
                lockSlim.ExitReadLock();
            }
        }

        public DataTable CurrentPage()
        {
            lockSlim.EnterReadLock();
            var table = currentTable;
            lockSlim.ExitReadLock();
            return table;
        }

        public bool IsCurrentPageInView(DataTable currentView)
        {
            lockSlim.EnterReadLock();
            var current = currentTable == currentView;
            lockSlim.ExitReadLock();
            return current;

        }

        public void IncrementTotalMissedMessages()
        {
            Interlocked.Increment(ref _totalMissedMessages);
        }

        public void UpdateOffsets()
        {
            lockSlim.EnterWriteLock();
            foreach (DataTable dataTable in pages)
            {
                foreach (DataRow dataTableRow in dataTable.Rows)
                {
                    dataTableRow.BeginEdit();
                    AnalogyLogMessage m = (AnalogyLogMessage)dataTableRow["Object"];
                    dataTableRow["Date"] = Utils.GetOffsetTime(m.Date);
                    dataTableRow.EndEdit();
                }
            }
            lockSlim.ExitWriteLock();
        }
    }

}
