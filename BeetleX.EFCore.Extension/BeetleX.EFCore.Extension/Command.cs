#if NETCOREAPP2_1
using BeetleX.Tracks;
#endif
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace BeetleX.EFCore.Extension
{

    public class Command
    {

        public Command(string text)
        {
            Text.Append(text);

        }

        private System.Text.StringBuilder mText = new System.Text.StringBuilder(256);

        public System.Text.StringBuilder Text
        {
            get
            {
                return mText;
            }

        }

        private CommandType mCommandType = CommandType.Text;

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

        public IDbCommand DbCommand
        {
            get;
            set;
        }
        private IList<Parameter> mParameters = new List<Parameter>();

        protected IList<Parameter> Parameters
        {
            get
            {
                return mParameters;
            }
        }

        public Command AddParameter(string name, object value)
        {
            return AddParameter(name, value, ParameterDirection.Input);

        }

        public Command AddParameter(Parameter parameter)
        {
            Parameters.Add(parameter);
            return this;
        }

        public Command AddParameter(string name, object value, ParameterDirection pd)
        {
            Parameter p = new Parameter();
            p.Name = name;
            p.Value = value;
            p.Direction = pd;
            Parameters.Add(p);
            return this;
        }

        public Command AddSqlText(string text)
        {
            Text.Append(text);
            return this;
        }

        public void ClearParameter()
        {
            Parameters.Clear();
        }

        public void Clean()
        {
            ClearParameter();
        }

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

        public DbCommand CreateCommand(DbConnection conn)
        {
#if NETCOREAPP2_1
            using (CodeTrackFactory.Track("CreateCommand", CodeTrackLevel.Function, null, "EFCore"))
            {
#endif
                DbCommand cmd = conn.CreateCommand();

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
                    cmd.Parameters.Add(cmdp);

                }
                return cmd;
#if NETCOREAPP2_1
            }
#endif
        }
    }
}
