select service, phone_number, sum(cost), sum(cost)*1.18 from MobileTable
where clientaccount='143017611' and month_event=4 
and year_event=2018 and phone_number='79262077082'
group by service, phone_number