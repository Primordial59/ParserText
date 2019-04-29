select year_event,  month_event, count(cost) from MobileTable
group by year_event, month_event