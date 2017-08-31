select * from MobileTable
inner join employee
on MobileTable.phone_number=employee.phone_number
where employee.phone_number='79028346408'
