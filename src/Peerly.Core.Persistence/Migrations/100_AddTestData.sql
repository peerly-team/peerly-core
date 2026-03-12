-- +goose Up
-- +goose StatementBegin

-- Seed teachers
INSERT INTO teachers (id, email, name, creation_time, update_time)
VALUES (1, 'ivanov@peerly.dev', 'Иван Иванов', NOW() - INTERVAL '90 days', NOW() - INTERVAL '2 days'),
       (2, 'petrova@peerly.dev', 'Анна Петрова', NOW() - INTERVAL '85 days', NOW() - INTERVAL '3 days'),
       (3, 'smirnov@peerly.dev', 'Дмитрий Смирнов', NOW() - INTERVAL '80 days', NOW() - INTERVAL '4 days'),
       (4, 'kozlova@peerly.dev', 'Елена Козлова', NOW() - INTERVAL '75 days', NOW() - INTERVAL '1 days');

-- Seed students
INSERT INTO students (id, email, name, creation_time, update_time)
VALUES (1, 'alex.morozov@student.peerly.dev', 'Алексей Морозов', NOW() - INTERVAL '70 days', NOW() - INTERVAL '1 days'),
       (2, 'maria.volkova@student.peerly.dev', 'Мария Волкова', NOW() - INTERVAL '69 days', NOW() - INTERVAL '2 days'),
       (3, 'nikita.sokolov@student.peerly.dev', 'Никита Соколов', NOW() - INTERVAL '68 days',
        NOW() - INTERVAL '3 days'),
       (4, 'olga.romanova@student.peerly.dev', 'Ольга Романова', NOW() - INTERVAL '67 days', NOW() - INTERVAL '2 days'),
       (5, 'egor.fedorov@student.peerly.dev', 'Егор Фёдоров', NOW() - INTERVAL '66 days', NOW() - INTERVAL '1 days'),
       (6, 'sofia.lebedeva@student.peerly.dev', 'София Лебедева', NOW() - INTERVAL '65 days',
        NOW() - INTERVAL '2 days'),
       (7, 'artem.kozlov@student.peerly.dev', 'Артём Козлов', NOW() - INTERVAL '64 days', NOW() - INTERVAL '4 days'),
       (8, 'alisa.novikova@student.peerly.dev', 'Алиса Новикова', NOW() - INTERVAL '63 days',
        NOW() - INTERVAL '2 days'),
       (9, 'maxim.belyaev@student.peerly.dev', 'Максим Беляев', NOW() - INTERVAL '62 days', NOW() - INTERVAL '3 days'),
       (10, 'polina.tarasova@student.peerly.dev', 'Полина Тарасова', NOW() - INTERVAL '61 days',
        NOW() - INTERVAL '2 days'),
       (11, 'kirill.orlov@student.peerly.dev', 'Кирилл Орлов', NOW() - INTERVAL '60 days', NOW() - INTERVAL '1 days'),
       (12, 'vera.zaitseva@student.peerly.dev', 'Вера Зайцева', NOW() - INTERVAL '59 days', NOW() - INTERVAL '1 days');

-- Seed courses
INSERT INTO courses (id, name, status, description, creation_time, update_time)
VALUES (1, 'C# Backend Academy', 'active', 'Практический курс по backend-разработке на C# и ASP.NET Core',
        NOW() - INTERVAL '50 days', NOW() - INTERVAL '1 days'),
       (2, 'PostgreSQL Deep Dive', 'active', 'Курс по проектированию БД, индексам, транзакциям и оптимизации запросов',
        NOW() - INTERVAL '48 days', NOW() - INTERVAL '2 days'),
       (3, 'System Design Basics', 'draft', 'Введение в проектирование распределённых систем',
        NOW() - INTERVAL '45 days', NOW() - INTERVAL '5 days');

-- Seed groups
INSERT INTO groups (id, name, course_id, creation_time, update_time)
VALUES (1, 'Backend Evening Group', 1, NOW() - INTERVAL '44 days', NOW() - INTERVAL '1 days'),
       (2, 'Backend Weekend Group', 1, NOW() - INTERVAL '43 days', NOW() - INTERVAL '1 days'),
       (3, 'SQL Morning Group', 2, NOW() - INTERVAL '42 days', NOW() - INTERVAL '2 days'),
       (4, 'SQL Intensive Group', 2, NOW() - INTERVAL '41 days', NOW() - INTERVAL '2 days'),
       (5, 'Architecture Pilot Group', 3, NOW() - INTERVAL '40 days', NOW() - INTERVAL '5 days');

-- Seed course_teachers
INSERT INTO course_teachers (course_id, teacher_id, creation_time)
VALUES (1, 1, NOW() - INTERVAL '40 days'),
       (1, 2, NOW() - INTERVAL '39 days'),
       (2, 2, NOW() - INTERVAL '38 days'),
       (2, 3, NOW() - INTERVAL '37 days'),
       (3, 4, NOW() - INTERVAL '36 days');

-- Seed group_teachers
INSERT INTO group_teachers (group_id, teacher_id, creation_time)
VALUES (1, 1, NOW() - INTERVAL '35 days'),
       (1, 2, NOW() - INTERVAL '35 days'),
       (2, 1, NOW() - INTERVAL '34 days'),
       (3, 2, NOW() - INTERVAL '33 days'),
       (3, 3, NOW() - INTERVAL '33 days'),
       (4, 3, NOW() - INTERVAL '32 days'),
       (5, 4, NOW() - INTERVAL '31 days');

-- Seed group_students
INSERT INTO group_students (group_id, student_id, creation_time)
VALUES (1, 1, NOW() - INTERVAL '30 days'),
       (1, 2, NOW() - INTERVAL '30 days'),
       (1, 3, NOW() - INTERVAL '30 days'),
       (1, 4, NOW() - INTERVAL '30 days'),
       (2, 5, NOW() - INTERVAL '29 days'),
       (2, 6, NOW() - INTERVAL '29 days'),
       (2, 7, NOW() - INTERVAL '29 days'),
       (3, 8, NOW() - INTERVAL '28 days'),
       (3, 9, NOW() - INTERVAL '28 days'),
       (3, 10, NOW() - INTERVAL '28 days'),
       (4, 11, NOW() - INTERVAL '27 days'),
       (4, 12, NOW() - INTERVAL '27 days'),
       (5, 2, NOW() - INTERVAL '26 days'),
       (5, 6, NOW() - INTERVAL '26 days');

INSERT INTO files (id, storage_id, name, size, creation_time)
VALUES
    (1,  '11111111-1111-1111-1111-111111111001', 'backend-intro.pdf', 245760, NOW() - INTERVAL '25 days'),
    (2,  '11111111-1111-1111-1111-111111111002', 'backend-roadmap.pdf', 184320, NOW() - INTERVAL '25 days'),
    (3,  '11111111-1111-1111-1111-111111111003', 'sql-indexes.pdf', 221184, NOW() - INTERVAL '24 days'),
    (4,  '11111111-1111-1111-1111-111111111004', 'sql-transactions.pdf', 196608, NOW() - INTERVAL '24 days'),
    (5,  '11111111-1111-1111-1111-111111111005', 'system-design-overview.pdf', 262144, NOW() - INTERVAL '23 days'),
    (6,  '11111111-1111-1111-1111-111111111006', 'hw1-requirements.md', 16384, NOW() - INTERVAL '22 days'),
    (7,  '11111111-1111-1111-1111-111111111007', 'hw1-template.zip', 524288, NOW() - INTERVAL '22 days'),
    (8,  '11111111-1111-1111-1111-111111111008', 'hw2-db-schema.drawio', 98304, NOW() - INTERVAL '21 days'),
    (9,  '11111111-1111-1111-1111-111111111009', 'hw2-dataset.csv', 655360, NOW() - INTERVAL '21 days'),
    (10, '11111111-1111-1111-1111-111111111010', 'hw3-checklist.txt', 8192, NOW() - INTERVAL '20 days'),
    (11, '11111111-1111-1111-1111-111111111011', 'morozov-solution.zip', 712345, NOW() - INTERVAL '10 days'),
    (12, '11111111-1111-1111-1111-111111111012', 'volkova-solution.zip', 689123, NOW() - INTERVAL '10 days'),
    (13, '11111111-1111-1111-1111-111111111013', 'sokolov-solution.zip', 701245, NOW() - INTERVAL '9 days'),
    (14, '11111111-1111-1111-1111-111111111014', 'romanova-query.sql', 20480, NOW() - INTERVAL '9 days'),
    (15, '11111111-1111-1111-1111-111111111015', 'fedorov-query.sql', 19456, NOW() - INTERVAL '8 days'),
    (16, '11111111-1111-1111-1111-111111111016', 'lebedeva-analysis.pdf', 312456, NOW() - INTERVAL '8 days'),
    (17, '11111111-1111-1111-1111-111111111017', 'novikova-index-report.pdf', 278900, NOW() - INTERVAL '7 days'),
    (18, '11111111-1111-1111-1111-111111111018', 'belyaev-index-report.pdf', 281400, NOW() - INTERVAL '7 days'),
    (19, '11111111-1111-1111-1111-111111111019', 'orlov-final.docx', 156789, NOW() - INTERVAL '6 days'),
    (20, '11111111-1111-1111-1111-111111111020', 'zaitseva-final.docx', 148321, NOW() - INTERVAL '6 days');
-- Seed course_files
INSERT INTO course_files (course_id, file_id, teacher_id)
VALUES (1, 1, '1'),
       (1, 2, '2'),
       (2, 3, '2'),
       (2, 4, '3'),
       (3, 5, '4');

-- Seed homeworks
INSERT INTO homeworks
(id, course_id, group_id, teacher_id, name, status, amount_of_reviewers, description, checklist, deadline,
 review_deadline, creation_time, update_time)
VALUES (1, 1, 1, 1, 'ASP.NET Core REST API', 'published', 2, 'Сделать REST API для заметок с CRUD и авторизацией',
        '["Есть CRUD","Есть валидация","Есть авторизация"]', NOW() + INTERVAL '5 days', NOW() + INTERVAL '10 days',
        NOW() - INTERVAL '20 days', NOW() - INTERVAL '2 days'),
       (2, 1, 2, 1, 'Background Jobs', 'published', 2, 'Реализовать фоновые задачи и ретраи',
        '["Есть scheduler","Есть retry policy","Есть логирование"]', NOW() + INTERVAL '6 days',
        NOW() + INTERVAL '11 days', NOW() - INTERVAL '19 days', NOW() - INTERVAL '2 days'),
       (3, 2, 3, 2, 'SQL Index Analysis', 'published', 2, 'Подобрать индексы и сравнить планы запросов',
        '["Есть explain analyze","Есть сравнение без индекса","Есть выводы"]', NOW() + INTERVAL '4 days',
        NOW() + INTERVAL '9 days', NOW() - INTERVAL '18 days', NOW() - INTERVAL '1 days'),
       (4, 2, 4, 3, 'Transaction Isolation Demo', 'review', 1, 'Продемонстрировать уровни изоляции транзакций',
        '["Есть сценарий dirty read","Есть repeatable read","Есть выводы"]', NOW() - INTERVAL '1 days',
        NOW() + INTERVAL '3 days', NOW() - INTERVAL '17 days', NOW() - INTERVAL '1 days'),
       (5, 3, 5, 4, 'Design URL Shortener', 'draft', 2, 'Спроектировать сервис коротких ссылок',
        '["Есть API","Есть storage","Есть scaling notes"]', NOW() + INTERVAL '12 days', NOW() + INTERVAL '16 days',
        NOW() - INTERVAL '10 days', NOW() - INTERVAL '5 days'),
       (6, 1, 1, 2, 'Unit Tests for Services', 'closed', 2, 'Покрыть сервисы модульными тестами',
        '["Есть happy path","Есть edge cases","Есть mocks"]', NOW() - INTERVAL '10 days', NOW() - INTERVAL '5 days',
        NOW() - INTERVAL '30 days', NOW() - INTERVAL '4 days');

-- Seed homework_files
INSERT INTO homework_files (homework_id, file_id, teacher_id)
VALUES (1, 6, 1),
       (1, 7, 1),
       (3, 8, 2),
       (3, 9, 2),
       (4, 10, 3);

-- Seed homework_submissions
INSERT INTO homework_submissions (id, homework_id, student_id, comment, creation_time, update_time)
VALUES (1, 1, 1, 'Сделал API на ASP.NET Core с JWT auth', NOW() - INTERVAL '3 days', NOW() - INTERVAL '2 days'),
       (2, 1, 2, 'Добавила swagger, fluent validation и postgres', NOW() - INTERVAL '3 days',
        NOW() - INTERVAL '2 days'),
       (3, 1, 3, 'Реализовал CRUD и middleware для ошибок', NOW() - INTERVAL '2 days', NOW() - INTERVAL '1 days'),
       (4, 3, 8, 'Сравнил btree и hash индексы', NOW() - INTERVAL '2 days', NOW() - INTERVAL '1 days'),
       (5, 3, 9, 'Разобрал explain analyze на нескольких запросах', NOW() - INTERVAL '2 days',
        NOW() - INTERVAL '1 days'),
       (6, 3, 10, 'Подготовила отчёт с замерами', NOW() - INTERVAL '1 days', NOW() - INTERVAL '1 days'),
       (7, 4, 11, 'Показал read committed и repeatable read', NOW() - INTERVAL '4 days', NOW() - INTERVAL '2 days'),
       (8, 4, 12, 'Описала race condition сценарии', NOW() - INTERVAL '4 days', NOW() - INTERVAL '2 days'),
       (9, 6, 4, 'Добавила unit tests и мокирование репозиториев', NOW() - INTERVAL '12 days',
        NOW() - INTERVAL '8 days'),
       (10, 6, 2, 'Покрыла сервисы тестами и проверила edge cases', NOW() - INTERVAL '11 days',
        NOW() - INTERVAL '7 days');

-- Seed homework_submission_files
INSERT INTO homework_submission_files (homework_submission_id, file_id)
VALUES (1, 11),
       (2, 12),
       (3, 13),
       (4, 17),
       (5, 18),
       (6, 16),
       (7, 19),
       (8, 20),
       (9, 14),
       (10, 15);

-- Seed homework_distributions
INSERT INTO homework_distributions (homework_id, distribute_in, creation_time, process_status, fail_count, process_time,
                                    taken_time, error)
VALUES (1, NOW() - INTERVAL '1 days', NOW() - INTERVAL '1 days', 'completed', 0, NOW() - INTERVAL '1 days',
        NOW() - INTERVAL '1 days', NULL),
       (3, NOW() - INTERVAL '12 hours', NOW() - INTERVAL '12 hours', 'completed', 0, NOW() - INTERVAL '12 hours',
        NOW() - INTERVAL '12 hours', NULL),
       (4, NOW() - INTERVAL '6 hours', NOW() - INTERVAL '6 hours', 'processing', 1, NOW() - INTERVAL '6 hours',
        NOW() - INTERVAL '5 hours', 'One reviewer skipped due to duplicate assignment');

-- Seed distribution_reviewers
INSERT INTO distribution_reviewers (homework_id, student_id)
VALUES (1, 1),
       (1, 2),
       (1, 3),
       (3, 8),
       (3, 9),
       (3, 10),
       (4, 11),
       (4, 12);

-- Seed review_submissions
INSERT INTO review_submissions (id, homework_id, student_id, mark, comment, creation_time)
VALUES (1, 1, 2, 9, 'Хорошая структура проекта и понятные контроллеры', NOW() - INTERVAL '1 days'),
       (2, 1, 3, 8, 'Не хватает пары негативных кейсов, но в целом хорошо', NOW() - INTERVAL '20 hours'),
       (3, 1, 1, 10, 'Отличная работа со swagger и валидацией', NOW() - INTERVAL '19 hours'),
       (4, 3, 8, 9, 'Хорошее сравнение планов выполнения', NOW() - INTERVAL '10 hours'),
       (5, 3, 9, 8, 'Есть выводы, но можно глубже по селективности индекса', NOW() - INTERVAL '9 hours'),
       (6, 4, 11, 7, 'Демо рабочее, но описание сценариев кратковато', NOW() - INTERVAL '4 hours');

INSERT INTO homework_submission_marks (homework_submission_id, reviewers_mark, teacher_mark, teacher_id, creation_time, update_time)
VALUES
    (1,  8,  9, 1, NOW() - INTERVAL '18 hours', NOW() - INTERVAL '12 hours'),
    (2, 10, 10, 1, NOW() - INTERVAL '17 hours', NOW() - INTERVAL '11 hours'),
    (4,  8,  9, 2, NOW() - INTERVAL '8 hours',  NOW() - INTERVAL '7 hours'),
    (5,  8,  8, 2, NOW() - INTERVAL '8 hours',  NOW() - INTERVAL '6 hours'),
    (7,  7,  8, 3, NOW() - INTERVAL '3 hours',  NOW() - INTERVAL '2 hours'),
    (9,  9,  9, 2, NOW() - INTERVAL '6 days',   NOW() - INTERVAL '5 days'),
    (10, 8,  9, 2, NOW() - INTERVAL '6 days',   NOW() - INTERVAL '5 days');

-- Fix sequences
SELECT setval('teachers_id_seq', (SELECT MAX(id) FROM teachers));
SELECT setval('students_id_seq', (SELECT MAX(id) FROM students));
SELECT setval('courses_id_seq', (SELECT MAX(id) FROM courses));
SELECT setval('groups_id_seq', (SELECT MAX(id) FROM groups));
SELECT setval('homeworks_id_seq', (SELECT MAX(id) FROM homeworks));
SELECT setval('homework_submissions_id_seq', (SELECT MAX(id) FROM homework_submissions));
SELECT setval('review_submissions_id_seq', (SELECT MAX(id) FROM review_submissions));
SELECT setval('files_id_seq', (SELECT MAX(id) FROM files));

-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin

DELETE
FROM homework_submission_marks
WHERE homework_submission_id IN (1, 2, 4, 5, 7, 9, 10);

DELETE
FROM review_submissions
WHERE id IN (1, 2, 3, 4, 5, 6);

DELETE
FROM distribution_reviewers
WHERE (homework_id, student_id) IN (
                                    (1, 1),
                                    (1, 2),
                                    (1, 3),
                                    (3, 8),
                                    (3, 9),
                                    (3, 10),
                                    (4, 11),
                                    (4, 12)
    );

DELETE
FROM homework_distributions
WHERE homework_id IN (1, 3, 4);

DELETE
FROM homework_submission_files
WHERE (homework_submission_id, file_id) IN (
                                            (1, 11),
                                            (2, 12),
                                            (3, 13),
                                            (4, 17),
                                            (5, 18),
                                            (6, 16),
                                            (7, 19),
                                            (8, 20),
                                            (9, 14),
                                            (10, 15)
    );

DELETE
FROM homework_submissions
WHERE id IN (1, 2, 3, 4, 5, 6, 7, 8, 9, 10);

DELETE
FROM homework_files
WHERE (homework_id, file_id, teacher_id) IN (
                                             (1, 6, 1),
                                             (1, 7, 1),
                                             (3, 8, 2),
                                             (3, 9, 2),
                                             (4, 10, 3)
    );

DELETE
FROM homeworks
WHERE id IN (1, 2, 3, 4, 5, 6);

DELETE
FROM course_files
WHERE (course_id, file_id, teacher_id) IN (
                                           (1, 1, '1'),
                                           (1, 2, '2'),
                                           (2, 3, '2'),
                                           (2, 4, '3'),
                                           (3, 5, '4')
    );

DELETE
FROM files
WHERE id IN (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20);

DELETE
FROM group_students
WHERE (group_id, student_id) IN (
                                 (1, 1),
                                 (1, 2),
                                 (1, 3),
                                 (1, 4),
                                 (2, 5),
                                 (2, 6),
                                 (2, 7),
                                 (3, 8),
                                 (3, 9),
                                 (3, 10),
                                 (4, 11),
                                 (4, 12),
                                 (5, 2),
                                 (5, 6)
    );

DELETE
FROM group_teachers
WHERE (group_id, teacher_id) IN (
                                 (1, 1),
                                 (1, 2),
                                 (2, 1),
                                 (3, 2),
                                 (3, 3),
                                 (4, 3),
                                 (5, 4)
    );

DELETE
FROM course_teachers
WHERE (course_id, teacher_id) IN (
                                  (1, 1),
                                  (1, 2),
                                  (2, 2),
                                  (2, 3),
                                  (3, 4)
    );

DELETE
FROM groups
WHERE id IN (1, 2, 3, 4, 5);

DELETE
FROM courses
WHERE id IN (1, 2, 3);

DELETE
FROM students
WHERE id IN (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12);

DELETE
FROM teachers
WHERE id IN (1, 2, 3, 4);

-- +goose StatementEnd
