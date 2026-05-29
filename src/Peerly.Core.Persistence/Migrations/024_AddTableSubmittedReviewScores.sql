-- +goose Up
-- +goose StatementBegin
create table submitted_review_scores
(
    id                  bigserial primary key not null,
    submitted_review_id bigint                not null,
    rubric_criteria_id  bigint                not null,
    score               int                   not null,
    comment             text
);
-- +goose StatementEnd

-- +goose Down
-- +goose StatementBegin
drop table submitted_review_scores;
-- +goose StatementEnd
