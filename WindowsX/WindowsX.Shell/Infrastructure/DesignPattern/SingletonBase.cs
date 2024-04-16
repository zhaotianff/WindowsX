using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsX.Shell.Infrastructure.DesignPattern
{
    public class SingletonBase<T> where T :new()
    {
        private static T instance;
        private static object obj = new object();

        public static T Instance
        {
            get
            {
                if(instance == null)
                {
                    lock(obj)
                    {
                        instance = new T();
                    }
                }
                return instance;
            }

        }

    }
}
