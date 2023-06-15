SELECT *
FROM my_table t
WHERE t.some_column IN
--[[INTERACTIVE
('some', 'made', 'up', 'values')
--]]
/*REPLACE-WITH(@p0)*/