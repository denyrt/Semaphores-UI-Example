using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace Threading
{
    public partial class MainForm : Form
    {
        private readonly static Semaphore _semaphore = new Semaphore(3, 3);
        private readonly static List<Thread> _threads = new List<Thread>();

        public MainForm()
        {
            InitializeComponent();            
        }

        private void buttonCreate_Click(object sender, EventArgs e)
        {           
            var threadName = textBoxName.Text;
            var seconds = (int)numericUpDownSeconds.Value;

            var thread = new Thread(() => DoWork(threadName, seconds));
            _threads.Add(thread);
            thread.Start();
        }
    
        private void DoWork(string threadName, int lifetime)
        {            
            var item = Init(threadName, lifetime);

            _semaphore.WaitOne();
            
            for (int i = 0; i < lifetime; ++i)
            {                
                Thread.Sleep(1000);
                Log(item, i + 1);
            }

            _semaphore.Release();

            var remove = new Action(() => listView.Items.Remove(item));

            if (InvokeRequired) Invoke(remove);

            else remove();

            _threads.Remove(Thread.CurrentThread);
        }

        private ListViewItem Init(string text, int lifetime)
        {
            var func = new Func<ListViewItem>(() =>
            {
                var item = listView.Items.Add(text);
                item.SubItems.Add(new ListViewItem.ListViewSubItem(item, "0") { Name = "ColumnWorkSecond" });
                item.SubItems.Add(new ListViewItem.ListViewSubItem(item, lifetime.ToString()) { Name = "ColumnLifetime" });                
                return item;
            });

            if (InvokeRequired) return Invoke(func) as ListViewItem;

            return func();
        }

        private void Log(ListViewItem item, int second)
        {
            var action = new Action(() => item.SubItems["ColumnWorkSecond"].Text = second.ToString());

            if (InvokeRequired) Invoke(action);

            else action();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (var thread in _threads)
                thread.Abort();
        }
    }
}
