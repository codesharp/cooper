using Android.App;
using Android.Views;
using Android.Widget;
using CooperDemo.Model;

namespace CooperDemo
{
    public class TaskFolderAdapter : ArrayAdapter<TaskFolder>
    {
        private Activity activity;

        public TaskFolderAdapter(Activity activity, int textViewResourceId, TaskFolder[] taskFolders)
            : base(activity, textViewResourceId, taskFolders)
        {
            this.activity = activity;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = this.GetItem(position) as TaskFolder;
            var view = (convertView ?? activity.LayoutInflater.Inflate(Resource.Layout.TaskFolderListItem, parent, false)) as LinearLayout;

            view.FindViewById<TextView>(Resource.Id.TaskFolderId).Text = item.ID.ToString();
            view.FindViewById<TextView>(Resource.Id.TaskFolderName).Text = item.Name;

            return view;
        }
    }
}

