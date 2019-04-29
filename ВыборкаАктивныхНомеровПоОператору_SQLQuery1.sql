select  DISTINCT(phone_number), operator from MobileTable
where operator='Megafon' and year_event=2018
group by phone_number, operator 
order by phone_number