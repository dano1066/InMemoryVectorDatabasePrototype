﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryVectorStore.Models.VectorStores
{
    public class DocumentAddResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public int ChunksAdded { get; set; }
    }
}
