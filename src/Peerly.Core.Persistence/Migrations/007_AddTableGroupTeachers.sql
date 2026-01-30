-- +goose Up
-- +goose StatementBegin
create table group_teachers
(
    id            bigserial primary key not null,
    group_id      bigint                not null,
    teacher_id    bigint                not null,
    creation_time timestamptz           not null
);
-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin
drop table group_teachers;
-- +goose StatementEnd
