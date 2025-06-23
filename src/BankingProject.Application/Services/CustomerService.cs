// ...existing code...

public async Task<Customer?> UpdateFieldsAsync(Guid customerId, Dictionary<string, string> updates)
{
    var customer = await GetCustomerByIdAsync(customerId);
    if (customer == null)
    {
        return null;
    }

    foreach (var update in updates)
    {
        var property = customer.GetType().GetProperty(update.Key, BindingFlags.Public | BindingFlags.Instance);
        if (property != null && property.CanWrite)
        {
            property.SetValue(customer, Convert.ChangeType(update.Value, property.PropertyType));
        }
    }

    customer.UpdatedAt = DateTime.UtcNow;
    await UpdateCustomerAsync(customer);
    return customer;
}
