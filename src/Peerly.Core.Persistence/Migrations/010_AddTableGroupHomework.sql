-- +goose Up
-- +goose StatementBegin
create table group_homeworks
(
    id              bigserial primary key not null,
    group_id        bigint                not null,
    name            text                  not null,
    description     text,
    checklist       text,
    status          text                  not null,
    deadline        timestamptz,
    review_deadline timestamptz,
    creation_time   timestamptz           not null,
    update_time     timestamptz
);
-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin
drop table group_homeworks;
-- +goose StatementEnd
