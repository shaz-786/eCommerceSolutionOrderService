using eCommerce.OrdersMicroservice.DataAccessLayer.Entities;
using eCommerce.OrdersMicroservice.DataAccessLayer.RepositoryContracts;
using MongoDB.Driver;


namespace eCommerce.OrdersMicroservie.DataAccessLayer.Repositories;


public class OrdersRepository : IOrdersRepository
{
    private readonly IMongoCollection<Order> _orders;
    private readonly string collectionName = "orders";

    public OrdersRepository(IMongoDatabase mongoDatabase) //inject imongodatabase type of service
    {
        _orders = mongoDatabase.GetCollection<Order>(collectionName);
    }


    public async Task<Order?> AddOrder(Order order)
    {
        order.OrderID = Guid.NewGuid();
        order._id = order.OrderID;//initialize it the same as orderId
        foreach (OrderItem orderItem in order.OrderItems)
        {
            orderItem._id = Guid.NewGuid();
        }
        await _orders.InsertOneAsync(order);
        return order;
    }


    public async Task<bool> DeleteOrder(Guid orderID)
    {//builder class frepresnts  a set of properties or methods that are used to create filterdefinition used for conditional query
        //and search defin, sort def, update def
        //filter reps equal , not equal and so on, it matches the order id if equal
        //it just define filterdefinitoon , so it can be used in query
        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.OrderID, orderID);

        //good practice to check for existence
        Order? existingOrder = (await _orders.FindAsync(filter)).FirstOrDefault();

        if (existingOrder == null)
        {
            return false;
        }

        //holds result of deletion in DeleteResult
        DeleteResult deleteResult = await _orders.DeleteOneAsync(filter);

        return deleteResult.DeletedCount > 0;
    }


    public async Task<Order?> GetOrderByCondition(FilterDefinition<Order> filter)
    {
        //we are receiving filterdef object so no need to create one - filter will be cretaed by service layer or controller
        return (await _orders.FindAsync(filter)).FirstOrDefault();
    }


    public async Task<IEnumerable<Order>> GetOrders()
    {// must send empty filter as it expects one, so no need to explicitly create it
        return (await _orders.FindAsync(Builders<Order>.Filter.Empty)).ToList();
    }


    public async Task<IEnumerable<Order?>> GetOrdersByCondition(FilterDefinition<Order> filter)
    {
        return (await _orders.FindAsync(filter)).ToList();
    }


    public async Task<Order?> UpdateOrder(Order order)
    {
        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.OrderID, order.OrderID);

        Order? existingOrder = (await _orders.FindAsync(filter)).FirstOrDefault();

        if (existingOrder == null)
        {
            return null;
        }
        order._id = existingOrder._id; //must do this programtically
        //ReplaceOneAsyc, replaces the order by order received as parameter
        ReplaceOneResult replaceOneResult = await _orders.ReplaceOneAsync(filter, order);

        return order;
    }
}
