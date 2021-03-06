﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetConfPl.Refactoring.Controllers;
using DotNetConfPl.Refactoring.Controllers.Persons;
using DotNetConfPl.Refactoring.Domain;
using DotNetConfPl.Refactoring.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DotNetConfPl.Refactoring.Application
{
    public class PersonService : IPersonService
    {
        private readonly CompaniesContext _companiesContext;

        public PersonService(CompaniesContext companiesContext)
        {
            _companiesContext = companiesContext;
        }

        public async Task<List<PersonDto>> GetAllPersons()
        {
            var employees = await _companiesContext.Persons
                .ToListAsync();

            return employees.Select(MapPersonToDto).ToList();
        }

        private PersonDto MapPersonToDto(Person person)
        {
            var employeeDto = new PersonDto();
            employeeDto.Id = person.Id;
            employeeDto.FirstName = person.FirstName;
            employeeDto.LastName = person.LastName;
            employeeDto.FullName = person.FullName;

            return employeeDto;
        }

        public async Task ChangePersonNames(Guid personId, string firstName, string lastName)
        {
            var person = await _companiesContext.Persons.SingleAsync(x => x.Id == personId);

            if (string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(lastName))
            {
                throw new BusinessException("Person must have defined first or last name");
            }

            person.FirstName = firstName;
            person.LastName = lastName;
            
            if (string.IsNullOrEmpty(firstName))
            {
                person.FullName = lastName;
            }
            else if (string.IsNullOrEmpty(lastName))
            {
                person.FullName = firstName;
            }
            else
            {
                person.FullName = $"{firstName} {lastName}";
            }

            await _companiesContext.SaveChangesAsync();
        }

        public async Task SetPersonAsEmployee(Guid personId, Guid companyId, string email, string phone)
        {
            if(await _companiesContext.Employees.AnyAsync(
                x => x.PersonId == personId && 
                     x.CompanyId == companyId && 
                     x.ActiveTo == null))
            {
                throw new BusinessException("Person can be active Employee of Company more than once");
            }
            
            var employee = new Employee();
            employee.Id = Guid.NewGuid();
            employee.PersonId = personId;
            employee.CompanyId = companyId;
            employee.ActiveFrom = DateTime.UtcNow;
            employee.Email = email;
            employee.Phone = phone;

            await _companiesContext.Employees.AddAsync(employee);

            await _companiesContext.SaveChangesAsync();
        }
    }
}