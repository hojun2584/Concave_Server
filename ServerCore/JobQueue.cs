using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCore
{
    public class JobQueue
    {
        public static JobQueue instance;
        Queue<Action> jobActions = new Queue<Action>();
        public static JobQueue Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new JobQueue();
                }
                return instance;
            }
        }


        public Action JobAction
        {
            get
            {
                return jobActions.Dequeue();
            }
            set
            {
                jobActions.Enqueue(value);
            }
        }



    }
}
