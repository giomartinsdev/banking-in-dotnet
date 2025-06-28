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

    /// <summary>
    /// Updates a Customer domain entity with values from UpdateCustomerRequest DTO
    /// </summary>
    /// <param name="customer">The customer domain entity to update</param>
    /// <param name="request">The update customer request DTO</param>
    /// <returns>Updated customer domain entity</returns>
    public static Customer UpdateFromRequest(this Customer customer, UpdateCustomerRequest request)
    {
        ArgumentNullException.ThrowIfNull(customer, nameof(customer));
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        // Update OpenPersonalInformation if any name fields are provided
        if (!string.IsNullOrWhiteSpace(request.FirstName) || !string.IsNullOrWhiteSpace(request.LastName))
        {
            var firstName = !string.IsNullOrWhiteSpace(request.FirstName)
                ? request.FirstName
                : customer.OpenPersonalInformation.FirstName;

            var lastName = !string.IsNullOrWhiteSpace(request.LastName)
                ? request.LastName
                : customer.OpenPersonalInformation.LastName;

            customer.OpenPersonalInformation = new OpenPersonalInformation(firstName, lastName);
        }

        // Update PrivatePersonalInformation if any contact fields are provided
        if (!string.IsNullOrWhiteSpace(request.Email) || !string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            var email = !string.IsNullOrWhiteSpace(request.Email)
                ? request.Email
                : customer.PrivatePersonalInformation.Email;

            var phoneNumber = !string.IsNullOrWhiteSpace(request.PhoneNumber)
                ? request.PhoneNumber
                : customer.PrivatePersonalInformation.PhoneNumber;

            // Keep existing merchant document and password
            customer.PrivatePersonalInformation = new PrivatePersonalInformation
            {
                Email = email,
                PhoneNumber = phoneNumber,
                MerchantDocument = customer.PrivatePersonalInformation.MerchantDocument,
                Password = customer.PrivatePersonalInformation.Password
            };
        }

        return customer;
    }
}
