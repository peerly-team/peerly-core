-- +goose Up
-- +goose StatementBegin
create table homework_distributions
(
    homework_id    bigint primary key not null,
    distribute_in  timestamptz        not null,
    creation_time  timestamptz        not null,
    process_status text               not null,
    fail_count     int                not null,
    process_time   timestamptz,
    taken_time     timestamptz,
    error          text
);
-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin
drop table homework_distributions;
-- +goose StatementEnd
