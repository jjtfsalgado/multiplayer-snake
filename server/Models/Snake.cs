using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MultiPlayerSnake
{
    public class Snake
    {
        public string id { get; set; }

        public int x { get; set; }

        public int y { get; set; }

        public int tail { get; set; }

        public string direction { get; set; }

        public string description { get; set; }

        public int speed { get; set; }

        public List<Pixel> trail { get; set; }
    }
}
