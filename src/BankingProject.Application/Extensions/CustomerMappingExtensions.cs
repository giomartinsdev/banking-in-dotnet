using BankingProject.Application.DTOs.CustomerDTOs;
using BankingProject.Domain.Context.CustomerAggregate.ValueObjects;

namespace BankingProject.Application.Extensions;

/// <summary>
/// Extension methods for mapping between Customer domain entities and DTOs
/// </summary>
public static class CustomerMappingExtensions
{
    /// <summary>
    /// Maps a Customer domain entity to CustomerResponse DTO
    /// </summary>
    /// <param name="customer">The customer domain entity</param>
    /// <returns>CustomerResponse DTO</returns>
    public static CustomerResponse ToResponse(this Customer customer)
    {
        ArgumentNullException.ThrowIfNull(customer, nameof(customer));

        return new CustomerResponse
        {
            Id = customer.Id,
            FirstName = customer.OpenPersonalInformation.FirstName,
            LastName = customer.OpenPersonalInformation.LastName,
            Email = customer.PrivatePersonalInformation.Email,
            PhoneNumber = customer.PrivatePersonalInformation.PhoneNumber,
            Balance = customer.Balance,
            CreatedAt = customer.ValidInformation.CreatedAt,
            IsActive = customer.ValidInformation.IsValid
        };
    }

    /// <summary>
    /// Maps CreateCustomerRequest DTO to Customer domain entity
    /// </summary>
    /// <param name="request">The create customer request DTO</param>
    /// <returns>Customer domain entity</returns>
    public static Customer ToDomainEntity(this CreateCustomerRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var openPersonalInformation = new OpenPersonalInformation(
            request.FirstName,
            request.LastName
        );

        var privatePersonalInformation = new PrivatePersonalInformation(
            request.Email,
            request.PhoneNumber,
            request.MerchantDocument,
            request.Password
        );

        var validInformation = new ValidInformation(isValid: true);

        return new Customer(
            openPersonalInformation,
            privatePersonalInformation,
            validInformation
        );
    }

    /// <summary>
    /// Maps a collection of Customer domain entities to CustomerResponse DTOs
    /// </summary>
    /// <param name="customers">The collection of customer domain entities</param>
    /// <returns>Collection of CustomerResponse DTOs</returns>
    public static IEnumerable<CustomerResponse> ToResponseList(this IEnumerable<Customer> customers)
    {
        ArgumentNullException.ThrowIfNull(customers, nameof(customers));

        return customers.Select(customer => customer.ToResponse());
    }
}
