-- +goose Up
-- +goose StatementBegin
alter table homeworks add column rubric_id bigint;
alter table homeworks drop column checklist;
-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin
alter table homeworks add column checklist text not null default '';
alter table homeworks drop column rubric_id;
-- +goose StatementEnd
