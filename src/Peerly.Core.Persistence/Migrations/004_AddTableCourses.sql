-- +goose Up
-- +goose StatementBegin
create table courses
(
    id            bigserial primary key not null,
    name          text                  not null,
    description   text,
    status        text                  not null, -- Draft, InProgress, Finished, Canceled, Deleted
    creation_time timestamptz           not null,
    update_time   timestamptz
);
-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin
drop table courses;
-- +goose StatementEnd
