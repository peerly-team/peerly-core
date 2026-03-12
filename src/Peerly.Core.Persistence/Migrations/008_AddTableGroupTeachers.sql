-- +goose Up
-- +goose StatementBegin
create table group_teachers
(
    group_id      bigint      not null,
    teacher_id    bigint      not null,
    creation_time timestamptz not null,

    primary key (group_id, teacher_id)
);
-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin
drop table group_teachers;
-- +goose StatementEnd
