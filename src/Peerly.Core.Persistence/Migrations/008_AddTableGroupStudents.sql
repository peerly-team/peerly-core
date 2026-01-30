-- +goose Up
-- +goose StatementBegin
create table group_students
(
    id            bigserial primary key not null,
    group_id      bigint                not null,
    student_id    bigint                not null,
    creation_time timestamptz           not null
);
-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin
drop table group_students;
-- +goose StatementEnd
