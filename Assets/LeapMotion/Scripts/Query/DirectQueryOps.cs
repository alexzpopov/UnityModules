﻿using System;
using System.Collections.Generic;

namespace Leap.Unity.Query {

  public partial struct QueryWrapper<QueryType, QueryOp> where QueryOp : IEnumerator<QueryType> {

    public bool Any() {
      using (thisAndConsume) {
        return _op.MoveNext();
      }
    }

    public bool Any(Func<QueryType, bool> predicate) {
      using (thisAndConsume) {
        while (_op.MoveNext()) {
          if (predicate(_op.Current)) {
            return true;
          }
        }
        return false;
      }
    }

    public bool All(Func<QueryType, bool> predicate) {
      using (thisAndConsume) {
        while (_op.MoveNext()) {
          if (!predicate(_op.Current)) {
            return false;
          }
        }
        return true;
      }
    }

    public bool Contains(QueryType instance) {
      using (thisAndConsume) {
        while (_op.MoveNext()) {
          if (_op.Current.Equals(instance)) {
            return true;
          }
        }
        return false;
      }
    }

    public int Count() {
      using (thisAndConsume) {
        int count = 0;
        while (_op.MoveNext()) {
          count++;
        }
        return count;
      }
    }

    public int Count(Func<QueryType, bool> predicate) {
      using (thisAndConsume) {
        int count = 0;
        while (_op.MoveNext()) {
          if (predicate(_op.Current)) {
            count++;
          }
        }
        return count;
      }
    }

    public QueryType First() {
      using (thisAndConsume) {
        if (!_op.MoveNext()) {
          throw new InvalidOperationException("The source query is empty.");
        }

        return _op.Current;
      }
    }

    public QueryType First(Func<QueryType, bool> predicate) {
      using (thisAndConsume) {
        while (true) {
          if (!_op.MoveNext()) {
            throw new InvalidOperationException("The source query did not have any elements that satisfied the predicate.");
          }

          if (predicate(_op.Current)) {
            return _op.Current;
          }
        }
      }
    }

    public QueryType FirstOrDefault() {
      using (thisAndConsume) {
        if (!_op.MoveNext()) {
          return default(QueryType);
        }

        return _op.Current;
      }
    }

    public QueryType FirstOrDefault(Func<QueryType, bool> predicate) {
      using (thisAndConsume) {
        while (true) {
          if (!_op.MoveNext()) {
            return default(QueryType);
          }

          if (predicate(_op.Current)) {
            return _op.Current;
          }
        }
      }
    }
  }
}
