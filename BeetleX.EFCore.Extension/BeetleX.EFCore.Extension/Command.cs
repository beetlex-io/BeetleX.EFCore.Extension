using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace BeetleX.EFCore.Extension
{
    /// <summary>
    /// 命令处理对象
    /// </summary>
    public class Command
    {
        /// <summary>
        /// 构建命令对象,并指定相应的SQL
        /// </summary>
        /// <param name="text">SQL语句</param>
        public Command(string text)
        {
            Text.Append(text);

        }

        private System.Text.StringBuilder mText = new System.Text.StringBuilder(256);
        /// <summary>
        /// 获取相应的SQL内容
        /// </summary>
        public System.Text.StringBuilder Text
        {
            get
            {
                return mText;
            }

        }

        private CommandType mCommandType = CommandType.Text;
        /// <summary>
        /// 获取或设置命令类型
        /// </summary>
        public CommandType CommandType
        {
            get
            {
                return mCommandType;
            }
            set
            {
                mCommandType = value;
            }

        }
        /// <summary>
        /// 获取或设置相应的数据库命令对象
        /// </summary>
        public IDbCommand DbCommand
        {
            get;
            set;
        }
        private IList<Parameter> mParameters = new List<Parameter>();
        /// <summary>
        /// 获取对应的参数集合
        /// </summary>
        protected IList<Parameter> Parameters
        {
            get
            {
                return mParameters;
            }
        }
        /// <summary>
        /// 添加指数名称和值的命令参数
        /// </summary>
        /// <param name="name">参数名</param>
        /// <param name="value">参数值</param>
        /// <returns>Command</returns>
        public Command AddParameter(string name, object value)
        {
            return AddParameter(name, value, ParameterDirection.Input);

        }
        /// <summary>
        /// 添加命令参数
        /// </summary>
        /// <param name="parameter">参数对象</param>
        /// <returns>Command</returns>
        public Command AddParameter(Parameter parameter)
        {
            Parameters.Add(parameter);
            return this;
        }
        /// <summary>
        /// 添加指数名称和值的命令参数
        /// </summary>
        /// <param name="name">参数名</param>
        /// <param name="value">参数值</param>
        /// <param name="pd">参数类型</param>
        /// <returns>Command</returns>
        public Command AddParameter(string name, object value, ParameterDirection pd)
        {
            Parameter p = new Parameter();
            p.Name = name;
            p.Value = value;
            p.Direction = pd;
            Parameters.Add(p);
            return this;
        }
        /// <summary>
        /// 添加SQL内容
        /// </summary>
        /// <param name="text">SQL内容</param>
        /// <returns>Command</returns>
        public Command AddSqlText(string text)
        {
            Text.Append(text);
            return this;
        }
        /// <summary>
        /// 清除所有参数
        /// </summary>
        public void ClearParameter()
        {
            Parameters.Clear();
        }
        /// <summary>
        /// 清除命令内部内容
        /// </summary>
        public void Clean()
        {
            ClearParameter();
        }
        /// <summary>
        /// 命令参数描述
        /// </summary>
        [Serializable]
        public class Parameter
        {
            private string mName;
            /// <summary>
            /// 参数名称
            /// </summary>
            public string Name
            {
                get
                {
                    return mName;
                }
                set
                {
                    mName = value;
                }
            }
            private object mValue;
            /// <summary>
            /// 参数值
            /// </summary>
            public object Value
            {
                get
                {
                    return mValue;
                }
                set
                {
                    mValue = value;
                }
            }
            private ParameterDirection mDirection = ParameterDirection.Input;
            /// <summary>
            /// 参数类型
            /// </summary>
            public ParameterDirection Direction
            {
                get
                {
                    return mDirection;
                }
                set
                {
                    mDirection = value;
                }
            }
        }
        /// <summary>
        /// 构建对应的数据库命令
        /// </summary>
        /// <param name="driver">数据库类型</param>
        /// <returns>IDbCommand</returns>
        public IDbCommand CreateCommand(DbConnection conn)
        {
            IDbCommand cmd = conn.CreateCommand();
            cmd.CommandText = Text.ToString();
            cmd.CommandType = CommandType;
            DbCommand = cmd;
            for (int i = 0; i < Parameters.Count; i++)
            {
                Parameter p = Parameters[i];
                var cmdp = cmd.CreateParameter();
                cmdp.ParameterName = p.Name;
                cmdp.Value = p.Value;
                cmdp.Direction = p.Direction;
                cmd.Parameters.Add(p);

            }
            return cmd;
        }
    }
}
