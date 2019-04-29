use MobileBase
select phone_number, sum(cost)*1.18 from MobileTable
where month_event='6' and year_event='2018' and clientaccount='73711191'
group by phone_number
order by sum(cost) DESC