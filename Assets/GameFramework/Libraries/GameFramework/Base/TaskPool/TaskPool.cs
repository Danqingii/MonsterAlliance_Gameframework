//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System.Collections.Generic;

namespace GameFramework
{
    /// <summary>
    /// 任务池。 
    /// </summary>
    /// <typeparam name="T">任务类型。</typeparam>
    internal sealed class TaskPool<T> where T : TaskBase
    {
        //注意!!
        //代理可以是多个 每个代理可持有一个任务 也可不持有任务
        //下载只接收代理的任务 也就是代理中的任务 才是下载任务
        //等待的任务会等待某一个代理结束下载 把自己添加进入代理
        //如此循环
        
        private readonly Stack<ITaskAgent<T>> m_FreeAgents;                        //自由代理池
        private readonly GameFrameworkLinkedList<ITaskAgent<T>> m_WorkingAgents;   //执行(下载)中的代理任务链表
        private readonly GameFrameworkLinkedList<T> m_WaitingTasks;                //等待中的任务链表
        private bool m_Paused;

        /// <summary>
        /// 初始化任务池的新实例。
        /// </summary>
        public TaskPool()
        {
            m_FreeAgents = new Stack<ITaskAgent<T>>();
            m_WorkingAgents = new GameFrameworkLinkedList<ITaskAgent<T>>();
            m_WaitingTasks = new GameFrameworkLinkedList<T>();
            m_Paused = false;
        }

        /// <summary>
        /// 获取或设置任务池是否被暂停。
        /// </summary>
        public bool Paused
        {
            get
            {
                return m_Paused;
            }
            set
            {
                m_Paused = value;
            }
        }

        /// <summary>
        /// 获取任务代理总数量。
        /// </summary>
        public int TotalAgentCount
        {
            get
            {
                return FreeAgentCount + WorkingAgentCount;
            }
        }

        /// <summary>
        /// 获取可用任务代理数量。
        /// </summary>
        public int FreeAgentCount
        {
            get
            {
                return m_FreeAgents.Count;
            }
        }

        /// <summary>
        /// 获取工作中任务代理数量。
        /// </summary>
        public int WorkingAgentCount
        {
            get
            {
                return m_WorkingAgents.Count;
            }
        }

        /// <summary>
        /// 获取等待任务数量。
        /// </summary>
        public int WaitingTaskCount
        {
            get
            {
                return m_WaitingTasks.Count;
            }
        }

        /// <summary>
        /// 任务池轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (m_Paused)
            {
                return;
            }

            //先轮询处理运行中的任务
            ProcessRunningTasks(elapseSeconds, realElapseSeconds);
            
            //然后轮询等待中的任务
            ProcessWaitingTasks(elapseSeconds, realElapseSeconds);
        }

        /// <summary>
        /// 关闭并清理任务池。
        /// </summary>
        public void Shutdown()
        {
            //移除 等待中的任务 + 执行中的任务
            RemoveAllTasks();

            //移除 自由的任务
            while (FreeAgentCount > 0)
            {
                m_FreeAgents.Pop().Shutdown();
            }
        }

        /// <summary>
        /// 增加任务代理。
        /// </summary>
        /// <param name="agent">要增加的任务代理。</param>
        public void AddAgent(ITaskAgent<T> agent)
        {
            if (agent == null)
            {
                throw new GameFrameworkException("Task agent is invalid.");
            }

            agent.Initialize();
            m_FreeAgents.Push(agent);
        }

        /// <summary>
        /// 根据任务的序列编号获取任务的信息。
        /// </summary>
        /// <param name="serialId">要获取信息的任务的序列编号。</param>
        /// <returns>任务的信息。</returns>
        public TaskInfo GetTaskInfo(int serialId)
        {
            //执行中的池
            foreach (ITaskAgent<T> workingAgent in m_WorkingAgents)
            {
                T workingTask = workingAgent.Task;
                if (workingTask.SerialId == serialId)
                {
                    return new TaskInfo(workingTask.SerialId, workingTask.Tag, workingTask.Priority, workingTask.UserData, workingTask.Done ? TaskStatus.Done : TaskStatus.Doing, workingTask.Description);
                }
            }

            //等待中的池
            foreach (T waitingTask in m_WaitingTasks)
            {
                if (waitingTask.SerialId == serialId)
                {
                    return new TaskInfo(waitingTask.SerialId, waitingTask.Tag, waitingTask.Priority, waitingTask.UserData, TaskStatus.Todo, waitingTask.Description);
                }
            }

            return default(TaskInfo);
        }

        /// <summary>
        /// 根据任务的标签获取任务的信息。
        /// </summary>
        /// <param name="tag">要获取信息的任务的标签。</param>
        /// <returns>任务的信息。</returns>
        public TaskInfo[] GetTaskInfos(string tag)
        {
            List<TaskInfo> results = new List<TaskInfo>();
            GetTaskInfos(tag, results);
            return results.ToArray();
        }

        /// <summary>
        /// 根据任务的标签获取任务的信息。
        /// </summary>
        /// <param name="tag">要获取信息的任务的标签。</param>
        /// <param name="results">任务的信息。</param>
        public void GetTaskInfos(string tag, List<TaskInfo> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (ITaskAgent<T> workingAgent in m_WorkingAgents)
            {
                T workingTask = workingAgent.Task;
                if (workingTask.Tag == tag)
                {
                    results.Add(new TaskInfo(workingTask.SerialId, workingTask.Tag, workingTask.Priority, workingTask.UserData, workingTask.Done ? TaskStatus.Done : TaskStatus.Doing, workingTask.Description));
                }
            }

            foreach (T waitingTask in m_WaitingTasks)
            {
                if (waitingTask.Tag == tag)
                {
                    results.Add(new TaskInfo(waitingTask.SerialId, waitingTask.Tag, waitingTask.Priority, waitingTask.UserData, TaskStatus.Todo, waitingTask.Description));
                }
            }
        }

        /// <summary>
        /// 获取所有任务的信息。
        /// </summary>
        /// <returns>所有任务的信息。</returns>
        public TaskInfo[] GetAllTaskInfos()
        {
            int index = 0;
            TaskInfo[] results = new TaskInfo[m_WorkingAgents.Count + m_WaitingTasks.Count];
            foreach (ITaskAgent<T> workingAgent in m_WorkingAgents)
            {
                T workingTask = workingAgent.Task;
                results[index++] = new TaskInfo(workingTask.SerialId, workingTask.Tag, workingTask.Priority, workingTask.UserData, workingTask.Done ? TaskStatus.Done : TaskStatus.Doing, workingTask.Description);
            }

            foreach (T waitingTask in m_WaitingTasks)
            {
                results[index++] = new TaskInfo(waitingTask.SerialId, waitingTask.Tag, waitingTask.Priority, waitingTask.UserData, TaskStatus.Todo, waitingTask.Description);
            }

            return results;
        }

        /// <summary>
        /// 获取所有任务的信息。
        /// </summary>
        /// <param name="results">所有任务的信息。</param>
        public void GetAllTaskInfos(List<TaskInfo> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (ITaskAgent<T> workingAgent in m_WorkingAgents)
            {
                T workingTask = workingAgent.Task;
                results.Add(new TaskInfo(workingTask.SerialId, workingTask.Tag, workingTask.Priority, workingTask.UserData, workingTask.Done ? TaskStatus.Done : TaskStatus.Doing, workingTask.Description));
            }

            foreach (T waitingTask in m_WaitingTasks)
            {
                results.Add(new TaskInfo(waitingTask.SerialId, waitingTask.Tag, waitingTask.Priority, waitingTask.UserData, TaskStatus.Todo, waitingTask.Description));
            }
        }

        /// <summary>
        /// 增加任务。
        /// </summary>
        /// <param name="task">要增加的任务。</param>
        public void AddTask(T task)
        {
            LinkedListNode<T> current = m_WaitingTasks.Last;
            while (current != null)
            {
                if (task.Priority <= current.Value.Priority)
                {
                    break;
                }

                current = current.Previous;
            }

            if (current != null)
            {
                m_WaitingTasks.AddAfter(current, task);
            }
            else
            {
                m_WaitingTasks.AddFirst(task);
            }
        }

        /// <summary>
        /// 根据任务的序列编号移除任务。
        /// </summary>
        /// <param name="serialId">要移除任务的序列编号。</param>
        /// <returns>是否移除任务成功。</returns>
        public bool RemoveTask(int serialId)
        {
            foreach (T task in m_WaitingTasks)
            {
                if (task.SerialId == serialId)
                {
                    m_WaitingTasks.Remove(task);
                    ReferencePool.Release(task);
                    return true;
                }
            }

            foreach (ITaskAgent<T> workingAgent in m_WorkingAgents)
            {
                if (workingAgent.Task.SerialId == serialId)
                {
                    T task = workingAgent.Task;
                    workingAgent.Reset();
                    m_FreeAgents.Push(workingAgent);
                    m_WorkingAgents.Remove(workingAgent);
                    ReferencePool.Release(task);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 根据任务的标签移除任务。
        /// </summary>
        /// <param name="tag">要移除任务的标签。</param>
        /// <returns>移除任务的数量。</returns>
        public int RemoveTasks(string tag)
        {
            int count = 0;

            LinkedListNode<T> currentWaitingTask = m_WaitingTasks.First;
            while (currentWaitingTask != null)
            {
                LinkedListNode<T> next = currentWaitingTask.Next;
                T task = currentWaitingTask.Value;
                if (task.Tag == tag)
                {
                    m_WaitingTasks.Remove(currentWaitingTask);
                    ReferencePool.Release(task);
                    count++;
                }

                currentWaitingTask = next;
            }

            LinkedListNode<ITaskAgent<T>> currentWorkingAgent = m_WorkingAgents.First;
            while (currentWorkingAgent != null)
            {
                LinkedListNode<ITaskAgent<T>> next = currentWorkingAgent.Next;
                ITaskAgent<T> workingAgent = currentWorkingAgent.Value;
                T task = workingAgent.Task;
                if (task.Tag == tag)
                {
                    workingAgent.Reset();
                    m_FreeAgents.Push(workingAgent);
                    ReferencePool.Release(task);
                    count++;
                }

                currentWorkingAgent = next;
            }

            return count;
        }

        /// <summary>
        /// 移除所有任务。
        /// </summary>
        /// <returns>移除任务的数量。</returns>
        public int RemoveAllTasks()
        {
            int count = m_WaitingTasks.Count + m_WorkingAgents.Count;

            foreach (T task in m_WaitingTasks)
            {
                ReferencePool.Release(task);
            }

            m_WaitingTasks.Clear();

            foreach (ITaskAgent<T> workingAgent in m_WorkingAgents)
            {
                T task = workingAgent.Task;
                workingAgent.Reset();
                m_FreeAgents.Push(workingAgent);
                ReferencePool.Release(task);
            }

            m_WorkingAgents.Clear();

            return count;
        }

        private void ProcessRunningTasks(float elapseSeconds, float realElapseSeconds)
        {
            LinkedListNode<ITaskAgent<T>> current = m_WorkingAgents.First;
            while (current != null)
            {
                T task = current.Value.Task;
                if (!task.Done)
                {
                    //当任务还未结束时 更新一次之后 继续让下一个任务去更新
                    current.Value.Update(elapseSeconds, realElapseSeconds);
                    current = current.Next;
                    continue;
                }
                //当前任务结束了
               
                LinkedListNode<ITaskAgent<T>> next = current.Next;
                current.Value.Reset();              //重置当前的代理
                m_FreeAgents.Push(current.Value);   //当前的代理回到自由池
                m_WorkingAgents.Remove(current);    //把当前任务从运行中的任务池中移出
                ReferencePool.Release(task);        
                current = next;
            }
        }

        private void ProcessWaitingTasks(float elapseSeconds, float realElapseSeconds)
        {
            LinkedListNode<T> current = m_WaitingTasks.First;                                //取出一个在等待中的任务
            
            while (current != null && FreeAgentCount > 0)                                    //只有在还有空代理位置的时候 才执行
            {
                ITaskAgent<T> agent = m_FreeAgents.Pop();                                    //取出一个空代理
                LinkedListNode<ITaskAgent<T>> agentNode = m_WorkingAgents.AddLast(agent);    //把空代理添加进到下载任务代理中 
                T task = current.Value;                                                      
                LinkedListNode<T> next = current.Next;                                       //把下一个等待任务获取一下 防止等一下链表断裂
                StartTaskStatus status = agent.Start(task);                                  //把该任务添加进去代理  让代理开始下载
                
                //该任务可以直接完成 或者在该帧当前任务被暂停了 或者该任务有问题
                if (status == StartTaskStatus.Done || status == StartTaskStatus.HasToWait || status == StartTaskStatus.UnknownError)
                {
                    //重置当前代理回到自由代理池   把当前代理从下载代理链表中移除
                    agent.Reset();
                    m_FreeAgents.Push(agent);
                    m_WorkingAgents.Remove(agentNode);
                }

                //该任务可以直接完成 或者可以继续处理任务  或者该任务有问题
                if (status == StartTaskStatus.Done || status == StartTaskStatus.CanResume || status == StartTaskStatus.UnknownError)
                {
                    //从等待池中也移除
                    m_WaitingTasks.Remove(current);
                }

                //下一此判断 该任务可以直接完成 或者该任务有问题
                if (status == StartTaskStatus.Done || status == StartTaskStatus.UnknownError)
                {
                    //回到引用池
                    ReferencePool.Release(task);
                }

                current = next;
            }
        }
    }
}
