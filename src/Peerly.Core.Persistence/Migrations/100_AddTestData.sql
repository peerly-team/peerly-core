-- +goose Up
-- +goose StatementBegin

insert into courses (id, name, description, status, creation_time, update_time)
values
    (1, 'Go Basics', 'Введение в Go: переменные, функции, структуры.', 'Draft', '2025-01-10 10:00:00+00', null),
    (2, 'Go Concurrency', 'Горутины, каналы, select.', 'InProgress', '2025-01-12 09:30:00+00', '2025-01-15 14:00:00+00'),
    (3, 'PostgreSQL Basics', 'Основы SQL и PostgreSQL.', 'Finished', '2025-01-14 08:15:00+00', '2025-01-20 11:20:00+00'),
    (4, 'REST API on Go', 'Создание REST API на Go.', 'InProgress', '2025-01-16 12:00:00+00', '2025-01-18 16:45:00+00'),
    (5, 'Docker Essentials', 'Контейнеры, образы и compose.', 'Finished', '2025-01-18 07:45:00+00', '2025-01-25 10:10:00+00'),
    (6, 'Kubernetes Intro', 'Базовые сущности Kubernetes.', 'Draft', '2025-01-20 13:25:00+00', null),
    (7, 'Testing in Go', 'Unit-тесты, table-driven tests.', 'Draft', '2025-01-28 09:00:00+00', null),
    (8, 'Microservices', 'Подходы к микросервисной архитектуре.', 'InProgress', '2025-02-01 08:20:00+00', '2025-02-05 13:15:00+00');

insert into teachers (id, email, name, creation_time, update_time)
values
    (1, 'ivan.petrov@example.com', 'Иван Петров', '2025-01-10 09:00:00+00', null),
    (2, 'anna.smirnova@example.com', 'Анна Смирнова', '2025-01-11 09:15:00+00', '2025-01-20 10:00:00+00'),
    (3, 'dmitry.kozlov@example.com', 'Дмитрий Козлов', '2025-01-12 10:30:00+00', null),
    (4, 'elena.popova@example.com', 'Елена Попова', '2025-01-13 11:00:00+00', '2025-01-18 12:20:00+00'),
    (5, 'sergey.volkov@example.com', 'Сергей Волков', '2025-01-14 08:40:00+00', null),
    (6, 'olga.sokolova@example.com', 'Ольга Соколова', '2025-01-15 13:10:00+00', '2025-01-22 14:45:00+00');

insert into students (id, email, name, creation_time, update_time)
values
    (1, 'alex.ivanov@example.com', 'Алексей Иванов', '2025-02-01 09:00:00+00', null),
    (2, 'irina.morozova@example.com', 'Ирина Морозова', '2025-02-01 09:10:00+00', null),
    (3, 'pavel.novikov@example.com', 'Павел Новиков', '2025-02-01 09:20:00+00', '2025-02-05 10:00:00+00'),
    (4, 'tatiana.orlova@example.com', 'Татьяна Орлова', '2025-02-01 09:30:00+00', null),
    (5, 'roman.belov@example.com', 'Роман Белов', '2025-02-01 09:40:00+00', null),
    (6, 'sofia.egorova@example.com', 'София Егорова', '2025-02-01 09:50:00+00', '2025-02-06 11:15:00+00'),
    (7, 'maxim.zaitsev@example.com', 'Максим Зайцев', '2025-02-01 10:00:00+00', null),
    (8, 'alisa.pavlova@example.com', 'Алиса Павлова', '2025-02-01 10:10:00+00', null),
    (9, 'kirill.stepanov@example.com', 'Кирилл Степанов', '2025-02-01 10:20:00+00', null),
    (10, 'vera.bogdanova@example.com', 'Вера Богданова', '2025-02-01 10:30:00+00', '2025-02-07 12:00:00+00'),
    (11, 'artem.kiselev@example.com', 'Артем Киселев', '2025-02-01 10:40:00+00', null),
    (12, 'yulia.titova@example.com', 'Юлия Титова', '2025-02-01 10:50:00+00', null);

insert into groups (id, name, course_id, creation_time, update_time)
values
    (1, 'Go Basics A', 1, '2025-02-10 09:00:00+00', null),
    (2, 'Go Basics B', 1, '2025-02-10 09:20:00+00', null),
    (3, 'Go Concurrency A', 2, '2025-02-11 10:00:00+00', '2025-02-15 12:00:00+00'),
    (4, 'PostgreSQL A', 3, '2025-02-11 10:30:00+00', null),
    (5, 'REST API A', 4, '2025-02-12 11:00:00+00', null),
    (6, 'Docker A', 5, '2025-02-12 11:30:00+00', '2025-02-18 09:45:00+00'),
    (7, 'Kubernetes A', 6, '2025-02-13 12:00:00+00', null),
    (8, 'Testing in Go A', 7, '2025-02-13 12:20:00+00', null),
    (9, 'Microservices A', 8, '2025-02-14 13:00:00+00', '2025-02-20 15:10:00+00');

insert into course_teachers (id, course_id, teacher_id, creation_time)
values
    (1, 1, 1, '2025-02-15 09:00:00+00'),
    (2, 1, 2, '2025-02-15 09:05:00+00'),
    (3, 2, 1, '2025-02-15 09:10:00+00'),
    (4, 3, 3, '2025-02-15 09:15:00+00'),
    (5, 4, 2, '2025-02-15 09:20:00+00'),
    (6, 4, 5, '2025-02-15 09:25:00+00'),
    (7, 5, 6, '2025-02-15 09:30:00+00'),
    (8, 6, 4, '2025-02-15 09:35:00+00'),
    (9, 7, 1, '2025-02-15 09:40:00+00'),
    (10, 8, 5, '2025-02-15 09:45:00+00');

insert into group_teachers (id, group_id, teacher_id, creation_time)
values
    (1, 1, 1, '2025-02-16 09:00:00+00'),
    (2, 1, 2, '2025-02-16 09:05:00+00'),
    (3, 2, 2, '2025-02-16 09:10:00+00'),
    (4, 3, 1, '2025-02-16 09:15:00+00'),
    (5, 4, 3, '2025-02-16 09:20:00+00'),
    (6, 5, 2, '2025-02-16 09:25:00+00'),
    (7, 5, 5, '2025-02-16 09:30:00+00'),
    (8, 6, 6, '2025-02-16 09:35:00+00'),
    (9, 7, 4, '2025-02-16 09:40:00+00'),
    (10, 8, 1, '2025-02-16 09:45:00+00'),
    (11, 9, 5, '2025-02-16 09:50:00+00');

insert into group_students (id, group_id, student_id, creation_time)
values
    (1, 1, 1, '2025-02-17 09:00:00+00'),
    (2, 1, 2, '2025-02-17 09:01:00+00'),
    (3, 1, 3, '2025-02-17 09:02:00+00'),
    (4, 2, 4, '2025-02-17 09:03:00+00'),
    (5, 2, 5, '2025-02-17 09:04:00+00'),
    (6, 2, 6, '2025-02-17 09:05:00+00'),
    (7, 3, 7, '2025-02-17 09:06:00+00'),
    (8, 3, 8, '2025-02-17 09:07:00+00'),
    (9, 4, 9, '2025-02-17 09:08:00+00'),
    (10, 4, 10, '2025-02-17 09:09:00+00'),
    (11, 5, 11, '2025-02-17 09:10:00+00'),
    (12, 5, 12, '2025-02-17 09:11:00+00');

insert into homeworks (id, course_id, teacher_id, name, description, checklist, status, deadline, review_deadline, creation_time, update_time)
values
    (1, 1, 1, 'Go Variables', 'Практика по переменным и типам.', '1. Создать переменные; 2. Вывести значения; 3. Проверить типы', 'Published', '2025-03-10 20:00:00+00', '2025-03-15 20:00:00+00', '2025-03-01 10:00:00+00', null),
    (2, 1, 2, 'Go Functions', 'Практика по функциям.', '1. Написать 3 функции; 2. Покрыть тестами', 'Published', '2025-03-12 20:00:00+00', '2025-03-17 20:00:00+00', '2025-03-02 10:00:00+00', null),
    (3, 2, 1, 'Channels', 'Работа с каналами.', '1. Producer/consumer; 2. Select; 3. Timeout', 'Draft', '2025-03-18 20:00:00+00', '2025-03-22 20:00:00+00', '2025-03-03 10:00:00+00', null),
    (4, 3, 3, 'SQL Queries', 'Набор запросов по PostgreSQL.', '1. SELECT; 2. JOIN; 3. GROUP BY', 'Published', '2025-03-09 20:00:00+00', '2025-03-14 20:00:00+00', '2025-03-01 11:00:00+00', null),
    (5, 4, 2, 'REST CRUD', 'Реализовать CRUD API.', '1. GET; 2. POST; 3. PUT; 4. DELETE', 'Published', '2025-03-20 20:00:00+00', '2025-03-25 20:00:00+00', '2025-03-04 10:00:00+00', null),
    (6, 8, 5, 'Service Split', 'Выделить сервис и описать взаимодействие.', '1. Выделить границы; 2. Описать API', 'Closed', '2025-03-05 20:00:00+00', '2025-03-08 20:00:00+00', '2025-02-25 10:00:00+00', '2025-03-08 09:00:00+00');

insert into group_homeworks (id, group_id, name, description, checklist, status, deadline, review_deadline, creation_time, update_time)
values
    (1, 1, 'Go Variables - Group 1', 'Домашняя работа для группы Go Basics A.', '1. Переменные; 2. Константы; 3. Форматирование', 'Published', '2025-03-10 20:00:00+00', '2025-03-15 20:00:00+00', '2025-03-01 12:00:00+00', null),
    (2, 2, 'Go Functions - Group 2', 'Домашняя работа для группы Go Basics B.', '1. Функции; 2. Возврат ошибок', 'Published', '2025-03-12 20:00:00+00', '2025-03-17 20:00:00+00', '2025-03-02 12:00:00+00', null),
    (3, 3, 'Channels - Group 3', 'Домашняя работа по каналам.', '1. Channels; 2. Buffered channels; 3. Select', 'Draft', '2025-03-18 20:00:00+00', '2025-03-22 20:00:00+00', '2025-03-03 12:00:00+00', null),
    (4, 4, 'SQL Queries - Group 4', 'Практика SQL-запросов.', '1. JOIN; 2. Aggregates', 'Published', '2025-03-09 20:00:00+00', '2025-03-14 20:00:00+00', '2025-03-01 13:00:00+00', null),
    (5, 5, 'REST CRUD - Group 5', 'CRUD API для группы REST.', '1. CRUD; 2. Validation; 3. Errors', 'Published', '2025-03-20 20:00:00+00', '2025-03-25 20:00:00+00', '2025-03-04 12:00:00+00', null),
    (6, 9, 'Service Split - Group 9', 'Проектирование микросервиса.', '1. Boundaries; 2. Endpoints', 'Closed', '2025-03-05 20:00:00+00', '2025-03-08 20:00:00+00', '2025-02-25 12:00:00+00', '2025-03-08 09:00:00+00');

insert into student_homeworks (id, student_id, group_homework_id, date, status, mark, creation_time, update_time)
values
    (1, 1, 1, '2025-03-09 18:00:00+00', 'Submitted', 85, '2025-03-01 12:10:00+00', '2025-03-09 18:00:00+00'),
    (2, 2, 1, '2025-03-10 17:30:00+00', 'Reviewed', 92, '2025-03-01 12:11:00+00', '2025-03-14 10:00:00+00'),
    (3, 3, 1, null, 'InProgress', null, '2025-03-01 12:12:00+00', '2025-03-05 09:00:00+00'),
    (4, 4, 2, '2025-03-11 19:10:00+00', 'Submitted', 78, '2025-03-02 12:10:00+00', '2025-03-11 19:10:00+00'),
    (5, 5, 2, null, 'NotStarted', null, '2025-03-02 12:11:00+00', null),
    (6, 6, 2, '2025-03-12 18:40:00+00', 'Reviewed', 88, '2025-03-02 12:12:00+00', '2025-03-16 11:00:00+00'),
    (7, 7, 3, null, 'InProgress', null, '2025-03-03 12:10:00+00', null),
    (8, 8, 3, null, 'NotStarted', null, '2025-03-03 12:11:00+00', null),
    (9, 9, 4, '2025-03-08 16:20:00+00', 'Reviewed', 95, '2025-03-01 13:10:00+00', '2025-03-13 10:30:00+00'),
    (10, 10, 4, '2025-03-09 14:05:00+00', 'Submitted', 81, '2025-03-01 13:11:00+00', '2025-03-09 14:05:00+00'),
    (11, 11, 5, null, 'InProgress', null, '2025-03-04 12:10:00+00', null),
    (12, 12, 5, '2025-03-19 15:30:00+00', 'Submitted', 90, '2025-03-04 12:11:00+00', '2025-03-19 15:30:00+00');

insert into student_homework_marks (id, student_id, student_homework_id, mark, creation_time, update_time)
values
    (1, 1, 1, 85, '2025-03-14 09:00:00+00', null),
    (2, 2, 2, 92, '2025-03-14 10:00:00+00', '2025-03-14 10:05:00+00'),
    (3, 4, 4, 78, '2025-03-16 09:30:00+00', null),
    (4, 6, 6, 88, '2025-03-16 11:00:00+00', '2025-03-16 11:10:00+00'),
    (5, 9, 9, 95, '2025-03-13 10:30:00+00', null),
    (6, 10, 10, 81, '2025-03-13 11:00:00+00', null),
    (7, 12, 12, 90, '2025-03-24 12:00:00+00', '2025-03-24 12:05:00+00');

insert into files (id, name, extension, creation_time)
values
    (1, 'go-variables-task', 'pdf', '2025-03-01 08:00:00+00'),
    (2, 'go-functions-examples', 'md', '2025-03-02 08:00:00+00'),
    (3, 'channels-homework', 'pdf', '2025-03-03 08:00:00+00'),
    (4, 'sql-queries-template', 'sql', '2025-03-01 08:30:00+00'),
    (5, 'rest-crud-contract', 'yaml', '2025-03-04 08:00:00+00'),
    (6, 'service-split-diagram', 'png', '2025-02-25 08:00:00+00'),
    (7, 'student-ivanov-solution', 'zip', '2025-03-09 18:05:00+00'),
    (8, 'student-orlova-solution', 'zip', '2025-03-11 19:15:00+00'),
    (9, 'student-bogdanova-solution', 'sql', '2025-03-09 14:10:00+00'),
    (10, 'student-titova-solution', 'zip', '2025-03-19 15:35:00+00');

insert into homework_files (id, homework_id, file_id, creation_time)
values
    (1, 1, 1, '2025-03-01 08:10:00+00'),
    (2, 2, 2, '2025-03-02 08:10:00+00'),
    (3, 3, 3, '2025-03-03 08:10:00+00'),
    (4, 4, 4, '2025-03-01 08:40:00+00'),
    (5, 5, 5, '2025-03-04 08:10:00+00'),
    (6, 6, 6, '2025-02-25 08:10:00+00');

insert into student_homework_files (id, student_homework_id, file_id, creation_time)
values
    (1, 1, 7, '2025-03-09 18:06:00+00'),
    (2, 4, 8, '2025-03-11 19:16:00+00'),
    (3, 10, 9, '2025-03-09 14:11:00+00'),
    (4, 12, 10, '2025-03-19 15:36:00+00');

insert into teacher_homework_approvals (id, teacher_id, group_homework_id, creation_time)
values
    (1, 1, 1, '2025-03-01 12:30:00+00'),
    (2, 2, 2, '2025-03-02 12:30:00+00'),
    (3, 1, 3, '2025-03-03 12:30:00+00'),
    (4, 3, 4, '2025-03-01 13:30:00+00'),
    (5, 2, 5, '2025-03-04 12:30:00+00'),
    (6, 5, 6, '2025-02-25 12:30:00+00');

select setval('courses_id_seq', (select max(id) from courses));
select setval('groups_id_seq', (select max(id) from groups));
select setval('course_teachers_id_seq', (select max(id) from course_teachers));
select setval('group_teachers_id_seq', (select max(id) from group_teachers));
select setval('group_students_id_seq', (select max(id) from group_students));
select setval('homeworks_id_seq', (select max(id) from homeworks));
select setval('group_homeworks_id_seq', (select max(id) from group_homeworks));
select setval('student_homeworks_id_seq', (select max(id) from student_homeworks));
select setval('student_homework_marks_id_seq', (select max(id) from student_homework_marks));
select setval('homework_files_id_seq', (select max(id) from homework_files));
select setval('student_homework_files_id_seq', (select max(id) from student_homework_files));
select setval('files_id_seq', (select max(id) from files));
select setval('teacher_homework_approvals_id_seq', (select max(id) from teacher_homework_approvals));

-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin

delete from teacher_homework_approvals where id between 1 and 6;
delete from student_homework_files where id between 1 and 4;
delete from homework_files where id between 1 and 6;
delete from files where id between 1 and 10;
delete from student_homework_marks where id between 1 and 7;
delete from student_homeworks where id between 1 and 12;
delete from group_homeworks where id between 1 and 6;
delete from homeworks where id between 1 and 6;
delete from group_students where id between 1 and 12;
delete from group_teachers where id between 1 and 11;
delete from course_teachers where id between 1 and 10;
delete from groups where id between 1 and 9;
delete from students where id between 1 and 12;
delete from teachers where id between 1 and 6;
delete from courses where id between 1 and 8;

-- +goose StatementEnd
