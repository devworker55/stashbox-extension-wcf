using System;

namespace stashbox.extension.wcf.sample.domain
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Condition Condition { get; set; }
        public Decimal Price { get; set; }
    }
}