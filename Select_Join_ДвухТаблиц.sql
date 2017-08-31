select * from employee
inner join MobileTable
on employee.phone_number=MobileTable.phone_number
where MobileTable.phone_number like '%408'

