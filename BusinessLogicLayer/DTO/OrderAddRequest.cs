namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;

public record OrderAddRequest(Guid UserID, DateTime OrderDate, List<OrderItemAddRequest> OrderItems)
{
    public OrderAddRequest() : this(default, default, default)// automapper will use this pARAMETERLESS constructor to create and object of this record
                                                              // and constructor chanining to call parametrized constructor to set default values
    {
    }
}