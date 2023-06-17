-- Not actually something important: REPLACE-WITH($env:SystemDrive)
SELECT *
FROM my_table t
WHERE t.some_column IN /*REPLACE-WITH(@p0)*/
