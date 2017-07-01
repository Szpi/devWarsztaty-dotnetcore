using System;
using System.Collections.Generic;

namespace Fibon.Api.Repository
{
    public interface IRepository
    {
        void Add(int number, int value);
        int? Get(int number);    
    }
    public class InMemoryRepository : IRepository
    {
        private readonly Dictionary<int,int> _storage = new Dictionary<int,int>();
        public void Add(int number, int value)
        {
            _storage[number] = value;
        }

        public int? Get(int number)
        {
            if (_storage.TryGetValue(number,out var value))
            {
                return value;
            }

            return null;
        }
    }
}