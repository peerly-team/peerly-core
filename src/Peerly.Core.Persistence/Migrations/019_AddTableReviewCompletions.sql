-- +goose Up
-- +goose StatementBegin
create table review_completions
(
    homework_id       bigint primary key not null,
    completion_time   timestamptz        not null,
    creation_time     timestamptz        not null,
    process_status    text               not null,
    fail_count        int                not null,
    process_time      timestamptz,
    taken_time        timestamptz,
    error             text
);
-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin
drop table review_completions;
-- +goose StatementEnd
