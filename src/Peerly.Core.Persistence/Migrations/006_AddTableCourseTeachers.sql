-- +goose Up
-- +goose StatementBegin
create table course_teachers
(
    id            bigserial primary key not null,
    course_id     bigint                not null,
    teacher_id    bigint                not null,
    creation_time timestamptz           not null
);
-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin
drop table course_teachers;
-- +goose StatementEnd
