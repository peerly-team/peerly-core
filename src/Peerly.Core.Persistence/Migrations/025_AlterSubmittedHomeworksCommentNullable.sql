-- +goose Up
-- +goose StatementBegin
alter table submitted_homeworks
    alter column comment drop not null;
-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin
alter table submitted_homeworks
    alter column comment set not null;
-- +goose StatementEnd
