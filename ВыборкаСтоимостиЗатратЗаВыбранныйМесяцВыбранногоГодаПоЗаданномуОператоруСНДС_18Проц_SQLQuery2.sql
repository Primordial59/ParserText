select  DISTINCT(phone_number), sum(cost)*1.18 as summa, operator from MobileTable
where operator='Megafon' and year_event=2018 and month_event=7 and clientaccount='73710851'
group by phone_number, operator
order by phone_number