-- +goose Up
-- +goose StatementBegin
create table rubric_criteria
(
    id               bigserial primary key not null,
    rubric_id        bigint                not null,
    name             text                  not null,
    description      text,
    max_score        int                   not null,
    comment_required boolean               not null,
    position         int                   not null,
    creation_time    timestamptz           not null,
    update_time      timestamptz
);
-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin
drop table rubric_criteria;
-- +goose StatementEnd
