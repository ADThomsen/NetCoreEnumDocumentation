using System.ComponentModel;

namespace NetCoreEnumDocumentation.Model
{
    /// <summary>
    /// Represents a basket
    /// </summary>
    public class Basket
    {
        /// <summary>
        /// The identifier of the basket
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// The owner of the basket
        /// </summary>
        /// <example>John Dow</example>
        public string Owner { get; set; }
        /// <summary>
        /// The current status of the basket
        /// </summary>
        public BasketStatus Status { get; set; }
    }

    /// <summary>
    /// XML Documentation for BasketStatus
    /// </summary>
    public enum BasketStatus
    {
        /// <summary>
        /// Still a basket - This won't show up in swagger
        /// </summary>
        [Description("Still a basket")]
        Basket = 0,
        /// <summary>
        /// Order has been placed - This won't show up in swagger
        /// </summary>
        [Description("Order has been placed")]
        Order = 1,
        /// <summary>
        /// Order has been invoiced - This won't show up in swagger
        /// </summary>
        [Description("Order has been invoiced")]
        Invoiced = 2,
        /// <summary>
        /// The order has been cancelled - This won't show up in swagger
        /// </summary>
        [Description("The order has been cancelled")]
        Cancelled = 3,
        /// <summary>
        /// The order is gone. This means.. well, we cannot find it - This won't show up in swagger
        /// </summary>
        [Description("The order is gone. This means.. well, we cannot find it")]
        Gone = 4
    }
}