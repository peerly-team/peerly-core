-- +goose Up
-- +goose StatementBegin
create table course_teachers
(
    course_id     bigint      not null,
    teacher_id    bigint      not null,
    creation_time timestamptz not null,

    primary key (course_id, teacher_id)
);
-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin
drop table course_teachers;
-- +goose StatementEnd
