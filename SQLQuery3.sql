select  phone_number, service, sum(cost) from MobileTable where clientaccount='73711191' and month_event=7
and service='subscriber'
group by phone_number, service