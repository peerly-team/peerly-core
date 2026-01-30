-- +goose Up
-- +goose StatementBegin
create table student_homeworks
(
    id                bigserial primary key not null,
    student_id        bigint                not null,
    group_homework_id bigint                not null,
    date              timestamptz,
    status            text                  not null,
    mark              integer,
    creation_time     timestamptz           not null,
    update_time       timestamptz
);
-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin
drop table student_homeworks;
-- +goose StatementEnd
