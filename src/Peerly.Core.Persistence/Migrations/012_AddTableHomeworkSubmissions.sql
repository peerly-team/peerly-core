-- +goose Up
-- +goose StatementBegin
create table homework_submissions
(
    id            bigserial primary key not null,
    homework_id   bigint                not null,
    student_id    bigint                not null,
    comment       text                  not null,
    creation_time timestamptz           not null,
    update_time   timestamptz
);
-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin
drop table homework_submissions;
-- +goose StatementEnd
