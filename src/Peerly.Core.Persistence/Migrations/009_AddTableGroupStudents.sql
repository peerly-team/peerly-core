-- +goose Up
-- +goose StatementBegin
create table group_students
(
    group_id      bigint      not null,
    student_id    bigint      not null,
    creation_time timestamptz not null,

    primary key (group_id, student_id)
);
-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin
drop table group_students;
-- +goose StatementEnd
