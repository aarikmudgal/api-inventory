using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eshop.api.inventory.Kafka
{
    public interface IInventoryConsumer
    {
        void Listen();
    }
}
